window.veilAudio = (() => {
    let ctx = null;
    const buffers = new Map(); // url -> AudioBuffer

    function ensureCtx() {
        if (!ctx) {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            ctx = new AudioContext();
        }
        return ctx;
    }

    async function resumeIfNeeded() {
        const c = ensureCtx();
        if (c.state === "suspended") {
            try {
                await c.resume();
            } catch { /* ignore */
            }
        }
    }

    async function getBuffer(url) {
        const c = ensureCtx();
        if (buffers.has(url)) return buffers.get(url);

        const resp = await fetch(url, {cache: "force-cache"});
        if (!resp.ok) throw new Error(`Failed to fetch audio: ${url}`);
        const data = await resp.arrayBuffer();
        const buf = await c.decodeAudioData(data.slice(0));
        buffers.set(url, buf);
        return buf;
    }

    // pitch matches XNA-ish semantics: +1 => one octave up, -1 => one octave down
    function pitchToRate(pitch) {
        return Math.pow(2, pitch);
    }

    async function playSfx(url, volume, pitch) {
        await resumeIfNeeded();

        const c = ensureCtx();
        const buffer = await getBuffer(url);

        const source = c.createBufferSource();
        source.buffer = buffer;
        source.playbackRate.value = pitchToRate(pitch ?? 0);

        const gain = c.createGain();
        gain.gain.value = Math.max(0, Math.min(1, volume ?? 1));

        source.connect(gain);
        gain.connect(c.destination);

        source.start();
    }

    return {playSfx};
})();
