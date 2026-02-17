using System;
using Gameplay.Entities.Enemies.Types;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Effects;

internal sealed class HulkerDrool(Hulker hulker, ISpawnEntity spawner) : IEntity
{
    private const int SpritePixelScale = 4;

    private const float BaseIntervalSeconds = 8f;
    private const float IntervalJitterSeconds = 2.0f; // +/- this much
    private readonly static Vector2 SpriteSize = new(32f, 32f);

    private readonly static Point[] DroolStartPixels =
    [
        new(12, 16),
        new(19, 16),
        new(13, 17),
        new(14, 17),
        new(17, 17),
        new(18, 17),
    ];

    private float _time;
    private float _acc;

    private readonly uint _seed = MakeSeed(hulker);

    private int _spawnIndex;
    private float _intervalSeconds = BaseIntervalSeconds;

    public bool MarkedForDeletion { get; private set; }

    public void Update(GameTime gameTime)
    {
        if (hulker.MarkedForDeletion)
        {
            MarkedForDeletion = true;
            return;
        }

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _time += dt;

        // Lazy init once, after we have a frame time.
        if (_spawnIndex == 0 && _time <= dt)
        {
            // Start “partway through” the interval so hulks don’t sync.
            _intervalSeconds = NextIntervalSeconds();
            _acc = Hash01(_seed ^ 0xB5297A4Du) * _intervalSeconds;
        }

        _acc += dt;

        while (_acc >= _intervalSeconds)
        {
            _acc -= _intervalSeconds;
            TrySpawn();

            _spawnIndex++;
            _intervalSeconds = NextIntervalSeconds();
        }
    }

    private void TrySpawn()
    {
        // Pick the start pixel from (seed, spawnIndex), not from time.
        var r = Hash01(_seed ^ ((uint)_spawnIndex * 0x9E3779B9u)); // r in [0,1)

        var i = (int)(r * DroolStartPixels.Length);
        if ((uint)i >= (uint)DroolStartPixels.Length)
            i = DroolStartPixels.Length - 1;

        spawner.Spawn(new DroolStrand(hulker, DroolStartPixels[i]));
    }

    private static Vector2 GetTopLeftWorld(Hulker hulker) => hulker.Position - SpriteSize * (SpritePixelScale * 0.5f);

    internal static Vector2 SpritePixelToWorld(Hulker hulker, Point pixel)
        => GetTopLeftWorld(hulker) + new Vector2(pixel.X * SpritePixelScale, pixel.Y * SpritePixelScale);

    private float NextIntervalSeconds()
    {
        var r = Hash01(_seed + (uint)_spawnIndex * 0x85EBCA6Bu); // [0,1)
        return BaseIntervalSeconds + (r * 2f - 1f) * IntervalJitterSeconds;
    }

    private static uint MakeSeed(Hulker hulker)
    {
        var x = (int)hulker.Position.X;
        var y = (int)hulker.Position.Y;
        return (uint)(x * 73856093) ^ (uint)(y * 19349663) ^ 0xA3C59AC3u;
    }

    private static float Hash01(uint x)
    {
        // integer hash -> [0,1)
        x ^= x >> 16;
        x *= 0x7feb352du;
        x ^= x >> 15;
        x *= 0x846ca68bu;
        x ^= x >> 16;

        return (x & 0x00ffffff) / 16777216f;
    }
}

internal sealed class DroolStrand(Hulker hulker, Point startPixel) : IEntity, IPrimitiveVisual
{
    private const float AverageMaxLengthPx = 6 * 4f;
    private readonly static TimeSpan Lifetime = TimeSpan.FromSeconds(4.5);

    private TimeSpan _t;

    private readonly float _maxLengthPx = ComputeMaxLengthPx(hulker, startPixel);

    public bool MarkedForDeletion { get; private set; }

    // Unused for drawing
    public Vector2 Position => hulker.Position;

    public float Layer => Layers.Projectiles;

    public void Update(GameTime gameTime)
    {
        _t += gameTime.ElapsedGameTime;

        if (hulker.MarkedForDeletion || _t >= Lifetime)
            MarkedForDeletion = true;
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        var t = (float)_t.TotalSeconds;
        var p = (float)(_t / Lifetime);

        var origin = HulkerDrool.SpritePixelToWorld(hulker, startPixel);

        // --- Length: reach full length quickly ---
        const float growSeconds = 0.85f;
        var g = MathHelper.Clamp(t / growSeconds, 0f, 1f);

        var fullLength = _maxLengthPx * EaseOutBack(g, 0.65f);
        if (fullLength < 0f) fullLength = 0f;
        if (fullLength > _maxLengthPx) fullLength = _maxLengthPx;

        if (fullLength <= 1f)
            return;

        // --- End behaviour: detach a droplet and retract the strand ---
        const float detachAt = 0.70f; // when the “event” starts
        const float dropDistancePx = 12f; // short, readable fall
        var end = SmoothStep(detachAt, 1.00f, p); // 0..1 over the end phase

        var strandLength = fullLength * (1f - end);

        var alpha = 1f - end;
        alpha *= alpha;

        const int widthPx = 4;

        static int RoundUpTo4(float v)
        {
            var i = (int)MathF.Round(v);
            return (i + 3) & ~3;
        }

        var x = (int)MathF.Round(origin.X - widthPx * 0.5f);
        var y = (int)MathF.Round(origin.Y);

        var h = RoundUpTo4(strandLength);
        if (h >= 4 && alpha > 0f)
            renderer.DrawRectangle(
                spriteBatch,
                new Rectangle(x, y, widthPx, h),
                ColorPalette.Lime * alpha,
                Layer);

        if (end > 0f && alpha > 0f)
        {
            var dropY = RoundUpTo4(end * dropDistancePx);
            var tipY = y + RoundUpTo4(fullLength);

            renderer.DrawRectangle(
                spriteBatch,
                new Rectangle(x, tipY + dropY - 4, 4, 4),
                ColorPalette.Lime * alpha,
                Layer);
        }
    }

    private static float ComputeMaxLengthPx(Hulker hulker, Point startPixel)
    {
        // Deterministic from spawn inputs (no Random needed).
        var seed = (uint)(
            ((startPixel.X & 255) << 0) ^
            ((startPixel.Y & 255) << 8) ^
            ((int)hulker.Position.X << 1) ^
            ((int)hulker.Position.Y << 3));

        var r0 = Hash01(seed);
        return AverageMaxLengthPx + (r0 * 2f - 1f) * 7f;
    }

    private static float EaseOutBack(float t, float overshoot)
    {
        var u = t - 1f;
        return 1f + u * u * ((overshoot + 1f) * u + overshoot);
    }

    private static float SmoothStep(float a, float b, float t)
    {
        t = MathHelper.Clamp((t - a) / (b - a), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    private static float Hash01(uint x)
    {
        x ^= x >> 16;
        x *= 0x7feb352du;
        x ^= x >> 15;
        x *= 0x846ca68bu;
        x ^= x >> 16;

        return (x & 0x00ffffff) / 16777216f;
    }
}