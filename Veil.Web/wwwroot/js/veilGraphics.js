window.veilGraphics = (() => {
    function getCanvas() {
        // KNI usually has a single canvas. If that ever changes, tighten this selector.
        return document.querySelector("canvas");
    }

    function getRenderMetrics() {
        const canvas = getCanvas();

        // Fallback to viewport if canvas isn't present yet.
        const rect = canvas ? canvas.getBoundingClientRect() : {width: window.innerWidth, height: window.innerHeight};

        const dpr = window.devicePixelRatio || 1;
        const cssWidth = Math.max(0, rect.width);
        const cssHeight = Math.max(0, rect.height);

        // Backbuffer (actual WebGL drawing buffer) should be CSS * DPR.
        const backBufferWidth = Math.max(1, Math.round(cssWidth * dpr));
        const backBufferHeight = Math.max(1, Math.round(cssHeight * dpr));

        return {
            cssWidth,
            cssHeight,
            dpr,
            backBufferWidth,
            backBufferHeight
        };
    }

    async function toggleFullscreen() {
        const canvas = getCanvas();
        const elem = canvas || document.documentElement;

        if (!document.fullscreenElement) {
            try {
                await elem.requestFullscreen();
            } catch {
                // ignore
            }
        } else {
            try {
                await document.exitFullscreen();
            } catch {
                // ignore
            }
        }
    }

    return {getRenderMetrics, toggleFullscreen};
})();
