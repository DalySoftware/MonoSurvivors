window.veilAudio = (() => {
    let ctx = null;

    // url -> AudioBuffer
    const buffers = new Map();

    // global nodes
    let masterGain = null;
    let sfxGain = null;

    // music nodes/state
    let musicGain = null;
    let musicFilter = null; // lowpass node (ready for later)
    let musicSource = null;

    function ensureCtx() {
        if (!ctx) {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            ctx = new AudioContext();

            // Build a small graph once:
            //  SFX:   (per-play gain) -> sfxGain -> masterGain -> destination
            //  Music: source -> musicGain -> musicFilter -> masterGain -> destination
            masterGain = ctx.createGain();
            masterGain.gain.value = 1.0;

            sfxGain = ctx.createGain();
            sfxGain.gain.value = 1.0;

            musicGain = ctx.createGain();
            musicGain.gain.value = 1.0;

            musicFilter = ctx.createBiquadFilter();
            musicFilter.type = "lowpass";
            // "Wide open" so it does nothing by default.
            musicFilter.frequency.value = 22050;

            sfxGain.connect(masterGain);
            musicGain.connect(musicFilter);
            musicFilter.connect(masterGain);
            masterGain.connect(ctx.destination);

            // Autoplay policies: try to resume the audio context on user gesture.
            const resume = () => {
                if (ctx && ctx.state === "suspended") {
                    ctx.resume().catch(() => { /* ignore */
                    });
                }
            };
            window.addEventListener("pointerdown", resume, {passive: true});
            window.addEventListener("keydown", resume);
        }

        return ctx;
    }

    async function resumeIfNeeded() {
        const c = ensureCtx();
        if (c.state === "suspended") {
            try {
                await c.resume();
            } catch {
                // ignore (browser may still block until gesture)
            }
        }
    }

    async function getBuffer(url) {
        const c = ensureCtx();
        const cached = buffers.get(url);
        if (cached) return cached;

        const resp = await fetch(url, {cache: "force-cache"});
        if (!resp.ok) throw new Error(`Failed to fetch audio: ${url} (${resp.status})`);

        const data = await resp.arrayBuffer();

        // decodeAudioData can be picky about detached buffers in some browsers;
        // slicing gives it a fresh ArrayBuffer.
        const buf = await c.decodeAudioData(data.slice(0));

        buffers.set(url, buf);
        return buf;
    }

    // pitch matches XNA-ish semantics: +1 => one octave up, -1 => one octave down
    function pitchToRate(pitch) {
        return Math.pow(2, pitch);
    }

    function clampZeroToOne(v) {
        v = +v;
        if (v < 0) return 0;
        if (v > 1) return 1;
        return v;
    }

    // -----------------------
    // SFX
    // -----------------------
    async function playSfx(url, volume, pitch) {
        await resumeIfNeeded();

        const c = ensureCtx();
        const buffer = await getBuffer(url);

        const source = c.createBufferSource();
        source.buffer = buffer;
        source.playbackRate.value = pitchToRate(pitch ?? 0);

        const gain = c.createGain();
        gain.gain.value = clampZeroToOne(volume ?? 1);

        source.connect(gain);
        gain.connect(sfxGain);

        source.start();
    }

    // -----------------------
    // MUSIC
    // -----------------------
    async function startMusic(url, volume) {
        await resumeIfNeeded();

        const c = ensureCtx();
        const buffer = await getBuffer(url);

        stopMusic(); // stop previous if any

        const source = c.createBufferSource();
        source.buffer = buffer;
        source.loop = true;

        // Apply initial volume before starting (avoids pop)
        setMusicVolume(volume ?? 1);

        source.connect(musicGain);
        source.start();

        musicSource = source;
    }

    function setMusicVolume(volume) {
        if (!ctx || !musicGain) return;

        const v = clampZeroToOne(volume ?? 1);

        // Smooth to avoid clicks.
        const t = ctx.currentTime;
        musicGain.gain.cancelScheduledValues(t);
        musicGain.gain.setTargetAtTime(v, t, 0.02);
    }

    function stopMusic() {
        if (!musicSource) return;

        try {
            musicSource.stop();
        } catch { /* ignore */
        }
        try {
            musicSource.disconnect();
        } catch { /* ignore */
        }
        musicSource = null;
    }

    return {
        playSfx,
        startMusic,
        setMusicVolume,
        stopMusic
    };
})();
