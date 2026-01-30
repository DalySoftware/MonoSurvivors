using System;
using System.Collections.Generic;
using Gameplay.Entities.Pooling;
using Gameplay.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Effects;

public sealed class GritBurst : IPrimitiveVisual, IEntity, IPoolableEntity
{
    // Tunables
    private const int MaxTotalClusters = 128;

    private const float DragPerSecond = 50f;

    private const float MinSpeed = 600f;
    private const float MaxSpeed = 800f;
    private const float SpeedScaleExponent = 0.3f; // compresses growth
    private const float MinSprayScale = 1.0f;
    private const float MaxSprayScale = 5.0f;

    private const int MinGrainSize = 4;
    private const int MaxGrainSize = 12;

    // Burst shaping
    private const float SpreadPerp = 16f; // overall burst width (world px)
    private const float FineJitter = 2f; // small noise so it isn’t too uniform

    // Shrink behaviour (4px steps over life)
    private const int ShrinkSteps = 3; // 0..3 steps
    private const int ShrinkStepPx = 4;

    // How many grains per cluster
    private const int MinGrains = 1;
    private const int MaxGrains = 3;

    private readonly static TimeSpan MinLife = TimeSpan.FromMilliseconds(120);
    private readonly static TimeSpan MaxLife = TimeSpan.FromMilliseconds(260);

    private readonly GritBurstPool _pool;
    private readonly GritCluster[] _clusters = new GritCluster[MaxTotalClusters];
    private int _count;

    private Color _baseColor;

    internal GritBurst(GritBurstPool pool)
    {
        _pool = pool;
    }

    public bool MarkedForDeletion { get; private set; }

    public Vector2 Position { get; private set; }
    public float Layer { get; private set; }

    public void Update(GameTime gameTime)
    {
        if (_count == 0)
        {
            MarkedForDeletion = true;
            return;
        }

        var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Keep list compact by swap-remove dead grits
        for (var i = _count - 1; i >= 0; i--)
        {
            ref var cluster = ref _clusters[i];

            cluster.Age += gameTime.ElapsedGameTime;
            if (cluster.Age >= cluster.Life)
            {
                _clusters[i] = _clusters[_count - 1];
                _count--;
                continue;
            }

            // Integrate
            cluster.Position += cluster.Velocity * deltaSeconds;

            // Drag
            var drag = 1f / (1f + DragPerSecond * deltaSeconds);
            cluster.Velocity *= drag;
        }

        if (_count == 0)
            MarkedForDeletion = true;
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        for (var i = 0; i < _count; i++)
        {
            ref readonly var cluster = ref _clusters[i];

            // Fraction of life (0 - 1)
            var lifeProgress =
                MathHelper.Clamp((float)(cluster.Age.TotalMilliseconds / cluster.Life.TotalMilliseconds), 0f, 1f);
            var lifeRemaining = 1f - lifeProgress;

            // Stepwise shrink over life (pixel-art friendly)
            var step = Math.Min(ShrinkSteps, lifeProgress * (ShrinkSteps + 1)); // 0..ShrinkSteps

            var size = (int)Math.Max(MinGrainSize, cluster.GrainSize - step * ShrinkStepPx);
            size = Quantize4(size);

            var grains = (int)Math.Max(1, cluster.Grains - step);

            // Dither threshold (0..1)
            var threshold = (cluster.Dither + 1) / 16f;

            // Make *bigger* grains die earlier so the burst "dithers down" over time
            var denomination = Math.Max(1f, MaxGrainSize - MinGrainSize);
            var size01 = (size - MinGrainSize) / denomination; // 0..1
            var sizeBias = 0.75f + 0.5f * size01; // 0.75..1.25 (bigger => harder to keep)

            if (lifeRemaining < threshold * sizeBias)
                continue;

            // Mostly opaque; avoid wispy alpha
            var color = _baseColor * cluster.Bright;

            DrawGritCluster(renderer, spriteBatch, cluster.Position, cluster.Direction, size, grains, color, Layer);
        }
    }

    private static void DrawGritCluster(
        PrimitiveRenderer renderer,
        SpriteBatch spriteBatch,
        Vector2 position,
        Vector2 direction,
        int size,
        int grains,
        Color color,
        float layer)
    {
        // Grain 0 at p
        DrawCenteredSquare(renderer, spriteBatch, position, size, color, layer);

        // Additional grains step out along dir (and a little perpendicular)
        var perpendicularDirection = new Vector2(-direction.Y, direction.X);

        for (var g = 1; g < grains; g++)
        {
            var along = 4f * g; // 4px steps
            var side = (g % 2 == 0 ? 1f : -1f) * 4f; // alternate sides

            var gp = position - direction * along + perpendicularDirection * side;

            // smaller secondary grains
            var s2 = Math.Max(4, size - 4 * g);
            DrawCenteredSquare(renderer, spriteBatch, gp, s2, color, layer);
        }
    }

    private static int Quantize4(float v) =>
        Math.Max(4, (int)MathF.Round(v / 4f) * 4);

    private static void DrawCenteredSquare(PrimitiveRenderer renderer, SpriteBatch spriteBatch, Vector2 p, float size,
        Color color, float layer)
    {
        var half = (int)MathF.Round(size * 0.5f);
        var x = (int)MathF.Round(p.X) - half;
        var y = (int)MathF.Round(p.Y) - half;
        var s = Math.Max(1, (int)MathF.Round(size));

        renderer.DrawRectangle(spriteBatch, new Rectangle(x, y, s, s), color, layer);
    }


    internal void Reinitialize(
        Vector2 position,
        Vector2 incomingVelocity,
        Vector2 inheritedVelocity,
        Color color)
    {
        MarkedForDeletion = false;

        Position = position;
        Layer = Layers.Projectiles;
        _baseColor = color;

        var incomingDir = SafeNormalize(incomingVelocity);
        if (incomingDir == Vector2.Zero)
            incomingDir = new Vector2(1, 0);

        var impactSpeed = incomingVelocity.Length(); // uses your impact velocity (bullet - enemy)
        var sprayScale = impactSpeed <= 0.0001f
            ? 1f
            : MathHelper.Clamp(MathF.Pow(impactSpeed, SpeedScaleExponent), MinSprayScale, MaxSprayScale);

        // spray backwards from incoming direction
        var baseDir = -incomingDir;
        var perp = new Vector2(-baseDir.Y, baseDir.X);

        var count = Math.Min(10, MaxTotalClusters);
        _count = count;

        for (var i = 0; i < count; i++)
        {
            // Stratified lane in [-1, +1], guaranteed monotonic with i (prevents crossovers)
            var u = (i + NextFloat()) / count * 2f - 1f;

            // Tie direction to lane (left lanes go left, right lanes go right)
            const float coneRadians = MathF.PI / 5f;
            var angle = u * (coneRadians * 0.5f);
            var direction = SafeNormalize(Rotate(baseDir, angle));
            _clusters[i].Direction = direction;

            var speed = MathHelper.Lerp(MinSpeed, MaxSpeed, NextFloat()) * sprayScale;
            _clusters[i].Velocity = direction * speed + inheritedVelocity;

            // Spawn position also tied to lane, so the burst has a coherent "fan" shape
            var jitter = new Vector2(
                (NextFloat() * 2f - 1f) * FineJitter,
                (NextFloat() * 2f - 1f) * FineJitter);

            var spawnPos = position + perp * (u * SpreadPerp) + jitter;


            var dither = (byte)(NextU() & 15); // 0..15
            var grainSize = (byte)Quantize4(MathHelper.Lerp(MinGrainSize, MaxGrainSize, NextFloat()));
            var grains = (byte)(MinGrains + NextFloat() * (MaxGrains - MinGrains + 1));
            var bright = 0.75f + 0.25f * NextFloat();

            var life = Lerp(MinLife, MaxLife, NextFloat());

            _clusters[i] = new GritCluster
            {
                Position = spawnPos,
                Velocity = direction * speed + inheritedVelocity,
                Life = life,
                Age = TimeSpan.Zero,
                Dither = dither,
                GrainSize = grainSize,
                Grains = grains,
                Bright = bright,
            };
        }
    }

    private TimeSpan Lerp(TimeSpan start, TimeSpan end, float amount) => (1 - amount) * start + end * amount;

    private static float NextFloat() => Random.Shared.NextSingle();
    private static uint NextU() => (uint)Random.Shared.Next();

    public void OnDespawned() => _pool.Return(this);

    private static Vector2 SafeNormalize(Vector2 v)
    {
        var ls = v.LengthSquared();
        return ls > 0.000001f ? v / MathF.Sqrt(ls) : Vector2.Zero;
    }

    private static Vector2 Rotate(Vector2 v, float radians)
    {
        var s = MathF.Sin(radians);
        var c = MathF.Cos(radians);
        return new Vector2(v.X * c - v.Y * s, v.X * s + v.Y * c);
    }

    private struct GritCluster
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Direction;
        public TimeSpan Life;
        public TimeSpan Age;

        public byte Dither; // 0..15
        public byte GrainSize; // px, already quantized to 4s
        public byte Grains; // 1..3
        public float Bright; // 0.75..1.0
    }
}

public sealed class GritBurstPool
{
    private readonly Stack<GritBurst> _pool = new();

    public GritBurst Get(
        Vector2 position,
        Vector2 incomingVelocity,
        Vector2 inheritedVelocity,
        Color color)
    {
        if (!_pool.TryPop(out var burst))
            burst = new GritBurst(this);

        burst.Reinitialize(position, incomingVelocity, inheritedVelocity, color);
        return burst;
    }

    internal void Return(GritBurst burst) => _pool.Push(burst);
}