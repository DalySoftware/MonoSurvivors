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
}