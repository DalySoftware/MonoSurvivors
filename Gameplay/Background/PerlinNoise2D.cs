using System;

namespace Gameplay.Background;

public sealed class PerlinNoise2D
{
    private readonly int[] _perm; // 512

    public PerlinNoise2D(int seed)
    {
        var p = new int[256];
        for (var i = 0; i < 256; i++) p[i] = i;

        var rng = new Random(seed);
        for (var i = 255; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (p[i], p[j]) = (p[j], p[i]);
        }

        _perm = new int[512];
        for (var i = 0; i < 512; i++) _perm[i] = p[i & 255];
    }

    /// <summary>
    ///     Returns noise in range [-1, +1].
    /// </summary>
    public float GetValue(float x, float y)
    {
        var xi = FastFloor(x) & 255;
        var yi = FastFloor(y) & 255;

        var xf = x - FastFloor(x);
        var yf = y - FastFloor(y);

        var u = Fade(xf);
        var v = Fade(yf);

        var aa = _perm[_perm[xi] + yi];
        var ab = _perm[_perm[xi] + yi + 1];
        var ba = _perm[_perm[xi + 1] + yi];
        var bb = _perm[_perm[xi + 1] + yi + 1];

        var x1 = MathHelper.Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
        var x2 = MathHelper.Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);

        return MathHelper.Lerp(x1, x2, v);
    }

    private static int FastFloor(float v)
    {
        var i = (int)v;
        return v < i ? i - 1 : i;
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

    // 2D gradient (classic Perlin)
    private static float Grad(int hash, float x, float y)
    {
        // 8 directions
        var h = hash & 7;
        var u = h < 4 ? x : y;
        var v = h < 4 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}