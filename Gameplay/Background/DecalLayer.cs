using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Rendering;
using Gameplay.Telemetry;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Background;

public sealed class DecalLayer
{
    private const int MaxCachedPatches = 1024;
    private const int SlotSize = 72;
    private const int PatchSize = SlotSize * 16;
    private readonly static int SlotsPerPatch = Math.Max(1, PatchSize / SlotSize);

    private readonly DecalPalette _palette = new();
    private readonly FifoCache<PatchKey, Patch> _patchCache = new(MaxCachedPatches);
    private readonly BackgroundDecalSpriteSheet _sheet;
    private readonly ChaseCamera _camera;
    private readonly PerformanceMetrics _perf;
    private readonly Func<PatchKey, Patch> _buildPatchFactory;

    private readonly PerlinNoise2D _noise;
    private readonly uint _runSeed;

    private readonly Vector2 _macroDensityOffset;
    private readonly Vector2 _microDensityOffset;

    private readonly Vector2 _tuftOffset;
    private readonly Vector2 _tuftDarkOffset;
    private readonly Vector2 _rocksOffset;

    public DecalLayer(BackgroundDecalSpriteSheet sheet, ChaseCamera camera, PerformanceMetrics perf)
    {
        _sheet = sheet;
        _camera = camera;
        _perf = perf;

        _buildPatchFactory = BuildPatch;

        _runSeed = (uint)Random.Shared.Next();
        _noise = new PerlinNoise2D((int)_runSeed);

        // One Perlin instance, multiple “channels” via different offsets.
        var channelRng = new Random((int)_runSeed);
        _microDensityOffset = NextOffset(channelRng);
        _macroDensityOffset = NextOffset(channelRng);
        _tuftOffset = NextOffset(channelRng);
        _tuftDarkOffset = NextOffset(channelRng);
        _rocksOffset = NextOffset(channelRng);
    }


    private static Vector2 NextOffset(Random rng)
    {
        // Large-ish offsets so channels don’t line up.
        const float span = 10_000f;
        return new Vector2(rng.NextSingle() * span, rng.NextSingle() * span);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        using var _ = _perf.MeasureProbe("Decal");

        // Expand by one slot so decals don’t “pop” at edges.
        var bounds = _camera.VisibleWorldBounds;
        bounds.Inflate(SlotSize, SlotSize);

        var startPatchX = FloorDiv(bounds.Left, PatchSize);
        var endPatchX = FloorDiv(bounds.Right - 1, PatchSize);
        var startPatchY = FloorDiv(bounds.Top, PatchSize);
        var endPatchY = FloorDiv(bounds.Bottom - 1, PatchSize);

        for (var py = startPatchY; py <= endPatchY; py++)
        for (var px = startPatchX; px <= endPatchX; px++)
        {
            var patch = _patchCache.GetOrAdd(new PatchKey(px, py), _buildPatchFactory);

            for (var i = 0; i < patch.Decals.Count; i++)
            {
                var d = patch.Decals[i];

                // Cull inside patch (cheap, avoids drawing lots of off-screen decals)
                if (!bounds.Contains((int)d.Position.X, (int)d.Position.Y))
                    continue;

                spriteBatch.Draw(d.Texture, d.Position, sourceRectangle: d.SourceRect, origin: d.Origin,
                    effects: d.Effects);
            }
        }
    }

    private static float ToUnit(float valueMinus1To1) => valueMinus1To1 * 0.5f + 0.5f;

    private static float SmoothStep(float edge0, float edge1, float x)
    {
        var t = MathHelper.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    private float GetDensityMultiplier(float worldX, float worldY)
    {
        const float microBlobSize = 768f;
        const float macroBlobSize = 8 * 768f;

        // Use the same masked-noise function but with floor=0 so we get a clean 0..1 “signal”.
        var macro01 = GetMaskedNoise(worldX, worldY, macroBlobSize, _macroDensityOffset, 0.35f, 0.75f, 1.3f, 0f);

        // Make micro smaller scale so you can actually see variation at 64px debug resolution.
        var micro01 = GetMaskedNoise(worldX, worldY, microBlobSize, _microDensityOffset, 0.25f, 0.70f, 1.2f, 0f);

        var macroFactor = MathHelper.Lerp(0.4f, 1.0f, macro01); // <- big lever
        var microFactor = MathHelper.Lerp(0.7f, 1.8f, micro01); // <- subtle lever

        return macroFactor * microFactor;
    }

    private float GetSpawnChance(float worldX, float worldY)
    {
        const float baseSpawnChance = 0.08f;
        var densityMultiplier = GetDensityMultiplier(worldX, worldY);
        return Math.Clamp(baseSpawnChance * densityMultiplier, 0f, 1f);
    }

    private float GetMaskedNoise(
        float worldX,
        float worldY,
        float blobSizePx,
        Vector2 offset,
        float edge0,
        float edge1,
        float power,
        float floor)
    {
        var nx = worldX / blobSizePx + offset.X;
        var ny = worldY / blobSizePx + offset.Y;

        var n01 = ToUnit(_noise.GetValue(nx, ny));
        var m = SmoothStep(edge0, edge1, n01);
        m = MathF.Pow(m, power);

        return floor + (1f - floor) * m;
    }


    private float GetThemeMask(DecalTheme theme, float worldX, float worldY)
    {
        const float blobSizePx = 12288;

        var offset = theme switch
        {
            DecalTheme.Tuft => _tuftOffset,
            DecalTheme.TuftDark => _tuftDarkOffset,
            DecalTheme.Rocks => _rocksOffset,
            _ => Vector2.Zero,
        };

        var nx = worldX / blobSizePx + offset.X;
        var ny = worldY / blobSizePx + offset.Y;

        var n01 = ToUnit(_noise.GetValue(nx, ny));
        var mask = SmoothStep(0.55f, 0.82f, n01);
        return MathF.Pow(mask, 2.0f);
    }


    private Patch BuildPatch(PatchKey key)
    {
        const uint salt = 0xA341316Cu;

        var patchX = key.X;
        var patchY = key.Y;
        var rng = new Random(MakeSeed(patchX, patchY, _runSeed ^ salt));

        // Patch origin in slot coordinates
        var patchSlotX = patchX * SlotsPerPatch;
        var patchSlotY = patchY * SlotsPerPatch;

        var decals = new List<DecalInstance>(64);

        for (var localY = 0; localY < SlotsPerPatch; localY++)
        for (var localX = 0; localX < SlotsPerPatch; localX++)
        {
            var sx = patchSlotX + localX;
            var sy = patchSlotY + localY;

            var slotMinX = (float)((long)sx * SlotSize);
            var slotMinY = (float)((long)sy * SlotSize);

            // Sample at slot centre so patches are stable and “blobby”
            var worldX = slotMinX + SlotSize * 0.5f;
            var worldY = slotMinY + SlotSize * 0.5f;

            if (rng.NextSingle() >= GetSpawnChance(worldX, worldY))
                continue;

            var tuftHot = GetThemeMask(DecalTheme.Tuft, worldX, worldY);
            var tuftDarkHot = GetThemeMask(DecalTheme.TuftDark, worldX, worldY);
            var rocksHot = GetThemeMask(DecalTheme.Rocks, worldX, worldY);

            var decal = PickDecal(rng.NextSingle(), tuftHot, tuftDarkHot, rocksHot);
            var (texture, src) = _sheet.GetFrameRectangle(decal.Decal);

            var origin = new Vector2(src.Width * 0.5f, src.Height * 0.5f);

            var x = slotMinX + origin.X + rng.NextSingle() * (SlotSize - src.Width);
            var y = slotMinY + origin.Y + rng.NextSingle() * (SlotSize - src.Height);

            var effects = decal.AllowFlipX && rng.NextSingle() < 0.5f
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            decals.Add(new DecalInstance(texture, new Vector2(x, y), src, origin, effects));
        }


        return new Patch(decals);
    }

    private DecalDefinition PickDecal(float roll01, float tuftHot, float tuftDarkHot, float rocksHot)
    {
        const float dominantBoost = 6.0f; // 4..10
        const float competingThemeMin = 0.15f; // 0.05..0.30
        const float dominanceThreshold = 0.25f; // 0.15..0.35

        // Find dominant themed hotspot at this position
        var dominantTheme = DecalTheme.Tuft;
        var dominantHot = tuftHot;

        if (tuftDarkHot > dominantHot)
        {
            dominantTheme = DecalTheme.TuftDark;
            dominantHot = tuftDarkHot;
        }

        if (rocksHot > dominantHot)
        {
            dominantTheme = DecalTheme.Rocks;
            dominantHot = rocksHot;
        }

        if (dominantHot < dominanceThreshold)
            dominantTheme = DecalTheme.None;

        float WeightFor(in DecalDefinition d)
        {
            var w = d.Weight;

            // Neutrals stay neutral; if no dominant hotspot, do nothing.
            if (dominantTheme == DecalTheme.None || d.Theme == DecalTheme.None)
                return w;

            // Dominant theme gets boosted; other themed decals get suppressed.
            return d.Theme == dominantTheme
                ? w * (1f + dominantBoost * dominantHot)
                : w * MathHelper.Lerp(1f, competingThemeMin, dominantHot);
        }

        // Pass 1: total
        var total = _palette.Decals.Sum(t => WeightFor(t));

        var roll = roll01 * total;

        // Pass 2: roulette
        for (var i = 0; i < _palette.Decals.Length; i++)
        {
            roll -= WeightFor(_palette.Decals[i]);
            if (roll <= 0f)
                return _palette.Decals[i];
        }

        return _palette.Decals[^1];
    }


    private static int FloorDiv(int value, int divisor)
    {
        // divisor > 0
        var q = value / divisor;
        if (value < 0 && value % divisor != 0) q--;
        return q;
    }

    private static int MakeSeed(int x, int y, uint seed)
    {
        ulong ux = (uint)x;
        ulong uy = (uint)y;
        var combined = (ux << 32) | uy;
        // Mix in global seed
        var seed64 = combined ^ (seed * 0x9E3779B97F4A7C15UL);
        return (int)(seed64 ^ (seed64 >> 32));
    }


    private readonly record struct PatchKey(int X, int Y);

    private readonly record struct Patch(List<DecalInstance> Decals);

    private readonly record struct DecalInstance(
        Texture2D Texture,
        Vector2 Position,
        Rectangle SourceRect,
        Vector2 Origin,
        SpriteEffects Effects);

    private sealed class FifoCache<TKey, TValue>(int capacity) where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _cache = new();
        private readonly Queue<TKey> _order = new();

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            if (_cache.TryGetValue(key, out var existing))
                return existing;

            var created = factory(key);
            _cache[key] = created;
            _order.Enqueue(key);

            while (_cache.Count > capacity && _order.Count > 0)
                _cache.Remove(_order.Dequeue());

            return created;
        }
    }
}