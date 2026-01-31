using System;
using System.Collections.Generic;
using ContentLibrary;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class PrimitiveRenderer(ContentManager content, GraphicsDevice graphicsDevice)
{
    public enum SoftCircleMaskType
    {
        OutsideTransparent,
        InsideTransparent,
    }

    private readonly static int[,] Bayer4 =
    {
        { 0, 8, 2, 10 },
        { 12, 4, 14, 6 },
        { 3, 11, 1, 9 },
        { 15, 7, 13, 5 },
    };
    private readonly Texture2D _pixelTexture = CreatePixelTexture(graphicsDevice);
    private readonly Texture2D _triangleTexture = content.Load<Texture2D>(Paths.Images.PanelTriangle);
    private readonly Dictionary<SoftCircleKey, Texture2D> _softCircleCache = new();

    private static Texture2D CreatePixelTexture(GraphicsDevice graphicsDevice)
    {
        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData([ColorPalette.White]);
        return texture;
    }

    public void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness,
        float layerDepth = 0f)
    {
        var distance = Vector2.Distance(start, end);
        var angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
        var origin = new Vector2(0f, 0.5f);

        spriteBatch.Draw(_pixelTexture, start, null, color, angle, origin,
            new Vector2(distance, thickness), SpriteEffects.None, layerDepth);
    }

    public void DrawLine(
        SpriteBatch spriteBatch,
        Vector2 start,
        float rotation,
        Vector2 scale, // (length, thickness)
        Color color,
        float layerDepth = 0f) =>
        spriteBatch.Draw(_pixelTexture, start, color, rotation: rotation, scale: scale, layerDepth: layerDepth,
            origin: new Vector2(0, 0.5f));

    public void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color, float layerDepth = 0f) =>
        spriteBatch.Draw(_pixelTexture, rect, color, layerDepth: layerDepth);

    /// <summary>
    ///     Default texture is 28x28. Hypotenuse from NW-SE. SW section transparent, NE section white.
    /// </summary>
    public void DrawTriangle(
        SpriteBatch spriteBatch,
        Vector2 position,
        Color color,
        float rotation = 0f,
        float layerDepth = 0f) => spriteBatch.Draw(
        _triangleTexture,
        position,
        color,
        rotation: rotation,
        layerDepth: layerDepth
    );

    public Texture2D CreateSoftCircle(
        int radius,
        SoftCircleMaskType maskType = SoftCircleMaskType.OutsideTransparent,
        decimal ditherStrength = 1.5m)
    {
        var key = new SoftCircleKey(radius, maskType, ditherStrength);
        return _softCircleCache.TryGetValue(key, out var cached)
            ? cached
            : _softCircleCache[key] = CreateSoftCircleUncached(radius, maskType, ditherStrength);
    }

    public void DrawSoftCircle(
        SpriteBatch spriteBatch,
        Vector2 center,
        int radius,
        Color color,
        SoftCircleMaskType maskType = SoftCircleMaskType.OutsideTransparent,
        decimal ditherStrength = 1.5m,
        float layerDepth = 0f)
    {
        var texture = CreateSoftCircle(radius, maskType, ditherStrength);
        spriteBatch.Draw(texture, center, color, origin: new Vector2(radius, radius), layerDepth: layerDepth);
    }

    private Texture2D CreateSoftCircleUncached(int radius,
        SoftCircleMaskType maskType = SoftCircleMaskType.OutsideTransparent, decimal ditherStrength = 1.5m)
    {
        const float softnessPx = 50f;
        const int pixelStep = 4;

        var size = radius * 2;
        var tex = new Texture2D(graphicsDevice, size, size);
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

                var bx = (x / pixelStep) & 3;
                var by = (y / pixelStep) & 3;
                var threshold =
                    (Bayer4[bx, by] / 16f - 0.5f) * (float)ditherStrength;

                var shouldFill = maskType == SoftCircleMaskType.OutsideTransparent ? t < threshold : t >= threshold;
                alpha = shouldFill ? 1f : 0f;
            }

            // Pre-multiply alpha
            var a = (byte)(alpha * 255);
            data[y * size + x] = new Color(a, a, a, a);
        }

        tex.SetData(data);
        return tex;
    }

    private static float SmoothStep(float a, float b, float t)
    {
        t = MathHelper.Clamp((t - a) / (b - a), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    private readonly record struct SoftCircleKey(int Radius, SoftCircleMaskType MaskType, decimal DitherStrength);
}