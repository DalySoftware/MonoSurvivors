using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLoop.Audio.Music.Catalog;

internal abstract class StemChoiceModule<TStem> : IMusicModule
    where TStem : struct, Enum
{
    private readonly static TStem[] StemValues = Enum.GetValues<TStem>();

    private int _holdBoundariesRemaining;
    private StemSelection<TStem> _selection;

    protected StemChoiceModule()
    {
        // Channel ids are the enum numeric values (0..N-1 contiguous by convention).
        Bindings = StemValues.ToDictionary(stem => (ushort)Convert.ToInt32(stem), StemKey);
    }

    protected MusicTier Tier { get; private set; } = MusicTier.Ambient;
    protected Random Random { get; } = new();

    public abstract TimeSpan LoopDuration { get; }

    public IReadOnlyDictionary<ushort, string> Bindings { get; }

    public void SetTier(MusicTier tier) => Tier = tier;

    public void OnLoopBoundary(long boundaryIndex)
    {
        if (_holdBoundariesRemaining > 0)
        {
            _holdBoundariesRemaining--;
            return;
        }

        _selection = PickSelection(Tier);

        _holdBoundariesRemaining = NextHold();
    }

    public void GetVolumes(Span<float> volumes)
    {
        if (volumes.Length != Bindings.Count)
            throw new ArgumentException($"Expected volumes length {Bindings.Count}, got {volumes.Length}.");

        volumes.Clear();

        if (_selection.Stems.Mask == 0)
            return;

        ApplyStemSelection(volumes, _selection);
    }

    private void ApplyStemSelection(Span<float> v, StemSelection<TStem> selection)
    {
        foreach (var stem in StemValues)
        {
            if (!selection.Contains(stem))
                continue;

            var level = selection.IsLow(stem) ? LowLevelFor(stem) : LevelFor(stem);

            v[(ushort)Convert.ToInt32(stem)] = level;
        }
    }

    protected virtual int NextHold() => Tier switch
    {
        // + 1 converts to exclusive bound
        MusicTier.Ambient => Random.Next(2, 4 + 1),
        MusicTier.Peak => Random.Next(1, 2 + 1),
        _ => Random.Next(1, 3 + 1),
    };

    protected abstract string StemKey(TStem stem);
    protected abstract StemSelection<TStem> PickSelection(MusicTier tier);
    protected abstract float LevelFor(TStem stem);
    protected abstract float LowLevelFor(TStem stem);
}

internal static class Choose
{
    internal static WeightedChoice<TStem> Stems<TStem>() where TStem : Enum => new();

    extension(WeightedChoice<Venezuela.Stems> c)
    {
        internal WeightedChoice<Venezuela.Stems> Mostly(params StemItem<Venezuela.Stems>[] items) =>
            c.Weight(30, items);
        internal WeightedChoice<Venezuela.Stems> Sometimes(params StemItem<Venezuela.Stems>[] items) =>
            c.Weight(10, items);
        internal WeightedChoice<Venezuela.Stems> Rarely(params StemItem<Venezuela.Stems>[] items) => c.Weight(4, items);
        internal WeightedChoice<Venezuela.Stems> VeryRarely(params StemItem<Venezuela.Stems>[] items) =>
            c.Weight(1, items);
    }

    extension(WeightedChoice<SwingyThing.Stems> c)
    {
        internal WeightedChoice<SwingyThing.Stems> Mostly(params StemItem<SwingyThing.Stems>[] items) =>
            c.Weight(30, items);
        internal WeightedChoice<SwingyThing.Stems> Sometimes(params StemItem<SwingyThing.Stems>[] items) =>
            c.Weight(10, items);
        internal WeightedChoice<SwingyThing.Stems> Rarely(params StemItem<SwingyThing.Stems>[] items) =>
            c.Weight(4, items);
        internal WeightedChoice<SwingyThing.Stems> VeryRarely(params StemItem<SwingyThing.Stems>[] items) =>
            c.Weight(1, items);
    }
}

internal static class Stem
{
    internal static StemItem<TStem> Low<TStem>(TStem stem) where TStem : Enum => new(stem, true);
}

internal sealed class WeightedChoice<TStem>
    where TStem : Enum
{
    private readonly List<(int Weight, StemSelection<TStem> Value)> _items = [];

    public WeightedChoice<TStem> Weight(int weight, params StemItem<TStem>[] items)
    {
        ulong onMask = 0;
        ulong lowMask = 0;

        foreach (var item in items)
        {
            var bit = 1UL << Convert.ToInt32(item.Stem);
            onMask |= bit;
            if (item.Low)
                lowMask |= bit;
        }

        _items.Add((weight, new StemSelection<TStem>(new StemSet<TStem>(onMask), new StemSet<TStem>(lowMask))));
        return this;
    }

    public StemSelection<TStem> Pick(Random rng)
    {
        var total = 0;
        for (var i = 0; i < _items.Count; i++)
            total += _items[i].Weight;

        var roll = rng.Next(total);
        for (var i = 0; i < _items.Count; i++)
        {
            roll -= _items[i].Weight;
            if (roll < 0)
                return _items[i].Value;
        }

        return _items[^1].Value;
    }
}

internal readonly record struct StemItem<TStem>(TStem Stem, bool Low) where TStem : Enum
{
    public static implicit operator StemItem<TStem>(TStem stem) => new(stem, false);
}

internal readonly record struct StemSelection<TStem>(StemSet<TStem> Stems, StemSet<TStem> Low) where TStem : Enum
{
    public bool Contains(TStem stem) => Stems.Contains(stem);
    public bool IsLow(TStem stem) => Low.Contains(stem);
}

internal readonly record struct StemSet<TStem>(ulong Mask) where TStem : Enum
{
    public bool Contains(TStem stem) => (Mask & (1UL << Convert.ToInt32(stem))) != 0;
}