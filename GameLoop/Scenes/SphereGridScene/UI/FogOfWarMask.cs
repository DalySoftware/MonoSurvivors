using System.Collections.Generic;
using System.Linq;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.SphereGridScene.UI;

internal sealed class FogOfWarMask
{
    private readonly static BlendState EraseBlend = new()
    {
        ColorSourceBlend = Blend.Zero,
        ColorDestinationBlend = Blend.InverseSourceAlpha,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.InverseSourceAlpha,
    };

    private readonly static int[,] Bayer4 =
    {
        { 0, 8, 2, 10 },
        { 12, 4, 14, 6 },
        { 3, 11, 1, 9 },
        { 15, 7, 13, 5 },
    };
    private readonly GraphicsDevice _graphics;
    private readonly RenderTarget2D _fogTarget;
    private readonly Texture2D _circle;
    private readonly SpriteBatch _spriteBatch;
    private readonly int _baseVisionRadius;
    private readonly float _layerDepth;

    private List<Vector2> _visibleCentres = [];

    public FogOfWarMask(GraphicsDevice graphics, int baseVisionRadius, float layerDepth)
    {
        _graphics = graphics;
        _baseVisionRadius = baseVisionRadius;
        _layerDepth = layerDepth;
        _spriteBatch = new SpriteBatch(graphics);

        var vp = graphics.Viewport;
        _fogTarget = new RenderTarget2D(
            graphics, vp.Width, vp.Height,
            false, SurfaceFormat.Color, DepthFormat.None);

        _circle = CreateSoftCircle(graphics, baseVisionRadius);
    }

    public void Rebuild(IEnumerable<Vector2> visibleCentres)
    {
        _visibleCentres = visibleCentres.ToList();

        _graphics.SetRenderTarget(_fogTarget);
        _graphics.Clear(Color.DarkSlateGray);

        // Punch holes
        _spriteBatch.Begin(blendState: EraseBlend);

        foreach (var pos in _visibleCentres)
            _spriteBatch.Draw(
                _circle,
                pos,
                origin: new Vector2(_baseVisionRadius),
                color: Color.White,
                layerDepth: _layerDepth + 0.01f);

        _spriteBatch.End();
        _graphics.SetRenderTarget(null);
    }

    public void Draw(SpriteBatch spriteBatch) =>
        spriteBatch.Draw(_fogTarget, Vector2.Zero, Color.DarkSlateGray, layerDepth: _layerDepth);

    public bool IsVisible(Vector2 screenPosition)
    {
        var radiusSquared = _baseVisionRadius * _baseVisionRadius;
        return _visibleCentres.Any(center => Vector2.DistanceSquared(center, screenPosition) <= radiusSquared);
    }

    private static Texture2D CreateSoftCircle(
        GraphicsDevice gd,
        int radius)
    {
        const float softnessPx = 50f;
        const float ditherStrength = 1.5f;
        const int pixelStep = 4;

        var size = radius * 2;
        var tex = new Texture2D(gd, size, size);
        var data = new Color[size * size];

        var center = new Vector2(radius);
        var inner = radius - softnessPx;
        var outer = radius;

        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            // ReSharper disable twice PossibleLossOfFraction
            // Quantize sampling position
            var qx = x / pixelStep * pixelStep + pixelStep * 0.5f;
            var qy = y / pixelStep * pixelStep + pixelStep * 0.5f;

            var d = Vector2.Distance(new Vector2(qx, qy), center);
            float alpha;

            if (d <= inner)
            {
                alpha = 1f;
            }
            else if (d >= outer)
            {
                alpha = 0f;
            }
            else
            {
                var t = 1f - SmoothStep(inner, outer, d);

                var bx = x / pixelStep & 3;
                var by = y / pixelStep & 3;

                var threshold =
                    (Bayer4[bx, by] / 16f - 0.5f) * ditherStrength;

                alpha = MathHelper.Clamp(t + threshold, 0f, 1f);
            }

            data[y * size + x] =
                new Color(255, 255, 255, alpha * 255);
        }

        tex.SetData(data);
        return tex;
    }


    private static float SmoothStep(float a, float b, float t)
    {
        t = MathHelper.Clamp((t - a) / (b - a), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
}