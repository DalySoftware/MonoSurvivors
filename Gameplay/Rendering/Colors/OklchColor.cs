using System;

namespace Gameplay.Rendering.Colors;

public readonly record struct OklchColor
{
    /// <param name="lightness">Perceptual lightness, 0–1</param>
    /// <param name="chroma">Chroma/saturation, 0–?. Usually 0–0.4 for sRGB gamut</param>
    /// <param name="hue">Hue angle, 0–360°</param>
    public OklchColor(float lightness, float chroma, float hue)
    {
        Lightness = lightness;
        Chroma = chroma;
        Hue = WrapHue(hue);
    }

    /// <summary>Perceptual lightness, 0–1</summary>
    public float Lightness { get; init; }

    /// <summary>Chroma/saturation, 0–?. Usually 0–0.4 for sRGB gamut</summary>
    public float Chroma { get; init; }

    /// <summary>Hue angle, 0–360°</summary>
    public float Hue { get; init; }

    private static float WrapHue(float h)
    {
        h %= 360f;
        if (h < 0) h += 360f;
        return h;
    }

    public static OklchColor FromColor(Color rgb)
    {
        // Convert RGB (0–255) to linear 0–1
        var r = SrgbToLinear(rgb.R / 255f);
        var g = SrgbToLinear(rgb.G / 255f);
        var b = SrgbToLinear(rgb.B / 255f);

        // Linear RGB → OKLab
        var l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
        var m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
        var s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;

        var rootL = CubeRoot(l);
        var rootM = CubeRoot(m);
        var rootS = CubeRoot(s);

        var lightness = 0.2104542553f * rootL + 0.7936177850f * rootM - 0.0040720468f * rootS;

        var aComponent = 1.9779984951f * rootL - 2.4285922050f * rootM + 0.4505937099f * rootS;
        var bComponent = 0.0259040371f * rootL + 0.7827717662f * rootM - 0.8086757660f * rootS;

        var chroma = MathF.Sqrt(aComponent * aComponent + bComponent * bComponent);
        var hue = MathF.Atan2(bComponent, aComponent) * (180f / MathF.PI); // radians → degrees

        return new OklchColor(lightness, chroma, hue);
    }

    public Color ToColor()
    {
        // Degrees → radians for trigonometric functions
        var radHue = Hue * (MathF.PI / 180f);
        var aComponent = Chroma * MathF.Cos(radHue);
        var bComponent = Chroma * MathF.Sin(radHue);

        // OKLab → LMS
        var lLinear = Lightness + 0.3963377774f * aComponent + 0.2158037573f * bComponent;
        var mLinear = Lightness - 0.1055613458f * aComponent - 0.0638541728f * bComponent;
        var sLinear = Lightness - 0.0894841775f * aComponent - 1.2914855480f * bComponent;

        // Cube to invert cube root from LMS → Linear RGB
        var lCubed = lLinear * lLinear * lLinear;
        var mCubed = mLinear * mLinear * mLinear;
        var sCubed = sLinear * sLinear * sLinear;

        // LMS → Linear RGB
        var rLinear = +4.0767416621f * lCubed - 3.3077115913f * mCubed + 0.2309699292f * sCubed;
        var gLinear = -1.2684380046f * lCubed + 2.6097574011f * mCubed - 0.3413193965f * sCubed;
        var bLinear = -0.0041960863f * lCubed - 0.7034186147f * mCubed + 1.7076147010f * sCubed;

        // Linear RGB → sRGB (0–255)
        var rSrgb = (byte)Math.Clamp((int)(LinearToSrgb(rLinear) * 255f), 0, 255);
        var gSrgb = (byte)Math.Clamp((int)(LinearToSrgb(gLinear) * 255f), 0, 255);
        var bSrgb = (byte)Math.Clamp((int)(LinearToSrgb(bLinear) * 255f), 0, 255);

        return new Color(rSrgb, gSrgb, bSrgb);
    }

    private static float SrgbToLinear(float x)
        => x <= 0.04045f ? x / 12.92f : MathF.Pow((x + 0.055f) / 1.055f, 2.4f);

    private static float LinearToSrgb(float x)
        => x <= 0.0031308f ? 12.92f * x : 1.055f * MathF.Pow(x, 1f / 2.4f) - 0.055f;

    private static float CubeRoot(float x)
        => MathF.CopySign(MathF.Pow(MathF.Abs(x), 1f / 3f), x);
}