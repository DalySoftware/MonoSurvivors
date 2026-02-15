window.veilAudio = (() => {
    // -----------------------
    // State
    // -----------------------
    /** @type {AudioContext|null} */
    let ctx = null;

    /** @type {Map<string, AudioBuffer>} */
    const buffers = new Map();

    // Global graph
    /** @type {GainNode|null} */
    let masterGain = null;
    /** @type {GainNode|null} */
    let sfxGain = null;

    // Music graph
    /** @type {GainNode|null} */
    let musicMasterGain = null; // duck/settings volume (multiplies all music)

    /** @type {Map<number, GainNode>} */
    const musicChannelGain = new Map(); // channel -> gain node (channel volume multiplier)

    /** @type {Map<number, GainNode>} */
    const musicTrackGain = new Map();   // channel -> gain node (current loop gain / fades)

    /** @type {Map<number, AudioBufferSourceNode|null>} */
    const musicSource = new Map();      // channel -> current source

    /** @type {null | { bindings: any[] }} */
    let pendingModuleStart = null;
    let unlockArmed = false;

    // -----------------------
    // Utility
    // -----------------------
    function clamp01(v) {
        v = +v;
        if (v < 0) return 0;
        if (v > 1) return 1;
        return v;
    }

    function pitchToRate(pitch) {
        return Math.pow(2, pitch);
    }

    function smoothSet(param, value, timeConstantSec = 0.02) {
        if (!ctx) return;
        const t = ctx.currentTime;
        param.cancelScheduledValues(t);
        param.setTargetAtTime(value, t, timeConstantSec);
    }

    function ensureCtx() {
        if (ctx) return ctx;

        const AudioContextCtor = window.AudioContext || window.webkitAudioContext;
        ctx = new AudioContextCtor();

        masterGain = ctx.createGain();
        masterGain.gain.value = 1.0;

        sfxGain = ctx.createGain();
        sfxGain.gain.value = 1.0;

        musicMasterGain = ctx.createGain();
        musicMasterGain.gain.value = 1.0;

        // Wire graph:
        // SFX:   (per-play gain) -> sfxGain -> masterGain -> destination
        // Music: source -> trackGain[channel] -> channelGain[channel] -> musicMasterGain -> masterGain -> destination
        sfxGain.connect(masterGain);
        musicMasterGain.connect(masterGain);
        masterGain.connect(ctx.destination);

        // Autoresume on user gesture
        const resume = () => {
            if (ctx && ctx.state === "suspended") {
                ctx.resume().catch(() => {
                });
            }
        };
        window.addEventListener("pointerdown", resume, {passive: true});
        window.addEventListener("keydown", resume);

        return ctx;
    }

    async function resumeIfNeeded() {
        const c = ensureCtx();
        if (c.state !== "suspended") return true;

        try {
            await c.resume();
            return c.state === "running";
        } catch {
            return false;
        }
    }

    async function getBuffer(url) {
        const c = ensureCtx();
        const cached = buffers.get(url);
        if (cached) return cached;

        const resp = await fetch(url, {cache: "force-cache"});
        if (!resp.ok) throw new Error(`Failed to fetch audio: ${url} (${resp.status})`);

        const ct = resp.headers.get("content-type") || "(none)";
        const data = await resp.arrayBuffer();

        try {
            const buf = await c.decodeAudioData(data.slice(0));
            buffers.set(url, buf);
            return buf;
        } catch (e) {
            // This is the key: identify the offender.
            throw new Error(
                `decodeAudioData failed for ${url} (content-type: ${ct}, bytes: ${data.byteLength})`
            );
        }
    }

    function ensureMusicChannel(channel) {
        const c = ensureCtx();
        const ch = channel | 0;

        let chGain = musicChannelGain.get(ch);
        if (!chGain) {
            chGain = c.createGain();
            chGain.gain.value = 1.0;
            chGain.connect(musicMasterGain);
            musicChannelGain.set(ch, chGain);
        }

        let trGain = musicTrackGain.get(ch);
        if (!trGain) {
            trGain = c.createGain();
            trGain.gain.value = 1.0;
            trGain.connect(chGain);
            musicTrackGain.set(ch, trGain);
        }

        if (!musicSource.has(ch)) {
            musicSource.set(ch, null);
        }

        return {ch, chGain, trGain};
    }

    // -----------------------
    // SFX
    // -----------------------
    async function playSfx(url, volume = 1, pitch = 0) {
        await resumeIfNeeded();

        const c = ensureCtx();
        const buffer = await getBuffer(url);

        const source = c.createBufferSource();
        source.buffer = buffer;
        source.playbackRate.value = pitchToRate(pitch);

        const gain = c.createGain();
        gain.gain.value = clamp01(volume);

        source.connect(gain);
        gain.connect(sfxGain);

        source.start();
    }

    // -----------------------
    // Music (per-channel)
    // -----------------------
    function stopMusicChannel(channel) {
        const ch = channel | 0;
        const src = musicSource.get(ch);
        if (!src) return;

        try {
            src.stop();
        } catch {
        }
        try {
            src.disconnect();
        } catch {
        }

        musicSource.set(ch, null);
    }

    function stopAllMusic() {
        for (const [ch] of musicSource) {
            stopMusicChannel(ch);
        }

        for (const g of musicTrackGain.values()) {
            g.gain.value = 0.0;
        }

        for (const g of musicChannelGain.values()) {
            g.gain.value = 1.0;
        }

        if (musicMasterGain) musicMasterGain.gain.value = 1.0;
    }

    function setMusicMasterVolume(volume) {
        if (!ctx || !musicMasterGain) return;
        smoothSet(musicMasterGain.gain, clamp01(volume));
    }

    function setMusicChannelVolume(channel, volume) {
        if (!ctx) return;
        const {chGain} = ensureMusicChannel(channel);
        smoothSet(chGain.gain, clamp01(volume));
    }

    function fadeOutAndStopChannel(channel, durationSeconds) {
        if (!ctx) return;

        const c = ensureCtx();
        const {ch, trGain} = ensureMusicChannel(channel);

        const src = musicSource.get(ch);
        if (!src) return;

        const dur = Math.max(0.01, +durationSeconds || 0.25);
        const t0 = c.currentTime;

        trGain.gain.cancelScheduledValues(t0);
        trGain.gain.setValueAtTime(trGain.gain.value, t0);
        trGain.gain.linearRampToValueAtTime(0.0, t0 + dur);

        const stopTime = t0 + dur + 0.03;
        try {
            src.stop(stopTime);
        } catch {
        }

        window.setTimeout(() => {
            // only clear if it’s still the same source
            if (musicSource.get(ch) === src) {
                try {
                    src.disconnect();
                } catch {
                }
                musicSource.set(ch, null);
            }
        }, Math.floor((dur + 0.1) * 1000));
    }

    async function startModule(bindings) {
        const ok = await resumeIfNeeded();
        if (!ok) {
            // Browser blocked resume (no gesture yet). Queue and arm unlock.
            pendingModuleStart = {bindings};
            armUnlockOnce();
            return; // <-- important: do not proceed to create/start sources
        }

        const c = ensureCtx();

        // bindings: [ { channel: number, url: string }, ... ]
        const buffersList = await Promise.all(bindings.map((b) => getBuffer(b.url)));

        const sources = [];
        for (let i = 0; i < bindings.length; i++) {
            const channel = bindings[i].channel | 0;
            const buffer = buffersList[i];

            const {ch, chGain, trGain} = ensureMusicChannel(channel);

            stopMusicChannel(ch);

            // Start silent by default; game will set per-channel volume after module start.
            chGain.gain.cancelScheduledValues(c.currentTime);
            chGain.gain.setValueAtTime(0.0, c.currentTime);

            trGain.gain.cancelScheduledValues(c.currentTime);
            trGain.gain.setValueAtTime(1.0, c.currentTime);

            const source = c.createBufferSource();
            source.buffer = buffer;
            source.loop = true;
            source.connect(trGain);

            musicSource.set(ch, source);
            sources.push(source);
        }

        const t0 = c.currentTime + 0.01;
        for (const s of sources) s.start(t0);
    }

    function armUnlockOnce() {
        if (unlockArmed) return;
        unlockArmed = true;

        const handler = async () => {
            try {
                if (!ctx) ensureCtx();
                if (ctx && ctx.state === "suspended") {
                    await ctx.resume();
                }
            } catch {
                // ignore
            }

            // If we successfully unlocked, replay the pending module start.
            if (ctx && ctx.state === "running" && pendingModuleStart) {
                const {bindings} = pendingModuleStart;
                pendingModuleStart = null;

                // Fire and forget; if this throws, it will show in console.
                startModule(bindings).catch(() => {
                });
            }

            window.removeEventListener("pointerdown", handler);
            window.removeEventListener("keydown", handler);
            unlockArmed = false;
        };

        window.addEventListener("pointerdown", handler, {passive: true, once: true});
        window.addEventListener("keydown", handler, {once: true});
    }

    async function tryResumeAudio() {
        const c = ensureCtx();
        if (c.state === "running") return true;
        try {
            await c.resume();
            return c.state === "running";
        } catch {
            return false;
        }
    }

    return {
        playSfx,

        stopMusicChannel,         // (channel) immediate
        stopAllMusic,

        setMusicMasterVolume,
        setMusicChannelVolume,
        fadeOutAndStopChannel,    // (channel, seconds)

        startModule,
        tryResumeAudio,
    };
})();
