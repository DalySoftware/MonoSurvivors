using System;

namespace Gameplay.Rendering.Colors;

public static class ColorOperations
{
    private static float WrapHue(float h)
    {
        const float twoPi = MathF.Tau;
        h %= twoPi;
        if (h < 0) h += twoPi;
        return h;
    }

    extension(Color color)
    {
        public float Lightness => OklchColor.FromColor(color).Lightness;

        public float Chroma => OklchColor.FromColor(color).Chroma;

        public float Hue => OklchColor.FromColor(color).Hue;

        public Color WithLightness(float delta) => (OklchColor.FromColor(color) with { Lightness = delta }).ToColor();
        public Color WithChroma(float delta) => (OklchColor.FromColor(color) with { Chroma = delta }).ToColor();
        public Color WithHue(float delta) => (OklchColor.FromColor(color) with { Hue = delta }).ToColor();

        public Color ShiftLightness(float delta) =>
            (OklchColor.FromColor(color) with { Lightness = color.Lightness + delta }).ToColor();

        public Color ShiftChroma(float delta) =>
            (OklchColor.FromColor(color) with { Chroma = color.Chroma + delta }).ToColor();

        public Color ShiftHue(float delta) =>
            (OklchColor.FromColor(color) with { Hue = WrapHue(color.Hue + delta) }).ToColor();

        /// <summary>
        ///     Set multiple OKLCH components at once (null means: leave unchanged).
        /// </summary>
        public Color WithOklch(float? l = null, float? c = null, float? h = null)
        {
            var o = OklchColor.FromColor(color);
            if (l.HasValue) o = o with { Lightness = l.Value };
            if (c.HasValue) o = o with { Chroma = c.Value };
            if (h.HasValue) o = o with { Hue = WrapHue(h.Value) };
            return o.ToColor();
        }
    }


    extension(Color)
    {
        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        // Returns shortest signed delta in [-pi, +pi]
        private static float ShortestHueDelta(float from, float to)
        {
            var d = WrapHue(to) - WrapHue(from);
            if (d > MathF.PI) d -= MathF.Tau;
            if (d < -MathF.PI) d += MathF.Tau;
            return d;
        }

        public static Color LerpOklch(Color from, Color to, float t, bool shortestHue = true)
        {
            t = Math.Clamp(t, 0f, 1f);

            var a = OklchColor.FromColor(from);
            var b = OklchColor.FromColor(to);

            // If one side is basically grey, keep the other's hue to avoid "random" hue swings.
            float ha;
            float hb;

            const float hueEpsilon = 1e-5f;
            if (a.Chroma < hueEpsilon && b.Chroma < hueEpsilon)
            {
                ha = hb = 0f; // hue irrelevant
            }
            else if (a.Chroma < hueEpsilon)
            {
                ha = hb = b.Hue;
            }
            else if (b.Chroma < hueEpsilon)
            {
                ha = hb = a.Hue;
            }
            else
            {
                ha = a.Hue;
                hb = b.Hue;
            }

            var h = shortestHue
                ? WrapHue(ha + Color.ShortestHueDelta(ha, hb) * t)
                : WrapHue(Color.Lerp(ha, hb, t)); // "long way" allowed

            var o = new OklchColor
            {
                Lightness = Color.Lerp(a.Lightness, b.Lightness, t), Chroma = Color.Lerp(a.Chroma, b.Chroma, t),
                Hue = h,
            };

            // Interpolate alpha separately (OKLCH is just RGB; alpha isn't part of it)
            var af = Color.Lerp(from.A / 255f, to.A / 255f, t);
            var aByte = (byte)Math.Clamp((int)MathF.Round(af * 255f), 0, 255);

            var rgb = o.ToColor();
            return new Color(rgb.R, rgb.G, rgb.B, aByte);
        }
    }
}