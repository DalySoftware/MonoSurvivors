using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GameLoop.Audio.Music.Catalog;

[SuppressMessage("ReSharper", "RedundantExplicitParamsArrayCreation")]
internal sealed class Venezuela : IMusicModule
{
    private readonly static WeightedChoice AmbientChoices =
        Choose.Stems()
            .Mostly([Stems.BassPad, Stem.Low(Stems.PercPulse)])
            .Sometimes([Stems.BassPad, Stems.PercHatsTight])
            .Sometimes([Stem.Low(Stems.BassA), Stems.PercHatsTight])
            .Sometimes([Stems.BassPad, Stems.PercHatsTight, Stem.Low(Stems.PercPulse)])
            .Rarely([Stem.Low(Stems.PercPulse)]);

    private readonly static WeightedChoice SoftChoices =
        Choose.Stems()
            .Sometimes([Stems.PercHatsTight, Stem.Low(Stems.BassA)])
            .Sometimes([Stems.PercHatsTight, Stem.Low(Stems.BassA2)])
            .Sometimes([Stems.PercHatsTight, Stem.Low(Stems.BassB)])
            .Sometimes([Stems.PercHatsTight, Stems.PercKickSparse, Stem.Low(Stems.BassA)])
            .Rarely([Stems.PercHatsTight, Stem.Low(Stems.BassA2), Stem.Low(Stems.PercPulse)])
            .Rarely([Stems.BassPad, Stems.PercHatsTight, Stem.Low(Stems.BassA)])
            .VeryRarely([Stems.PercHatsTight, Stems.PercKickSparse, Stem.Low(Stems.BassA), Stems.LeadA2])
            .VeryRarely([Stems.PercHatsTight, Stem.Low(Stems.BassA2), Stems.LeadB]);

    private readonly static WeightedChoice CoreChoices =
        Choose.Stems()
            .Sometimes([Stems.PercHatsTight, Stems.PercKickFour, Stems.BassA])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA2])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA, Stem.Low(Stems.PercPulse)])
            .Rarely([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB, Stems.PercSnare])
            .Rarely([Stems.BassPad, Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA2])
            .Rarely([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA2, Stem.Low(Stems.PercPulse), Stems.LeadA]);

    private readonly static WeightedChoice PeakChoices =
        Choose.Stems()
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB, Stems.PercPulse])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA2, Stems.PercPulse])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB, Stems.PercSnare])
            .Rarely([Stems.BassPad, Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA2, Stems.PercPulse, Stems.LeadB])
            .Rarely([Stems.BassPad, Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB, Stems.PercPulse, Stems.LeadA])
            .VeryRarely([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB, Stems.PercPulse, Stems.LeadA2]);

    private readonly static Stems[] StemValues = Enum.GetValues<Stems>();

    private MusicTier _tier = MusicTier.Ambient;
    private readonly Random _rng = new();
    private int _holdBoundariesRemaining;
    private StemSelection _selection;

    // 8 bars @ 125 BPM = 15.360s
    public TimeSpan LoopDuration { get; } = TimeSpan.FromSeconds(15.360);

    public IReadOnlyDictionary<ushort, string> Bindings { get; } =
        StemValues.ToDictionary(stem => (ushort)stem, StemKey);

    public void SetTier(MusicTier tier) => _tier = tier;

    public void OnLoopBoundary(long boundaryIndex)
    {
        if (_holdBoundariesRemaining > 0)
        {
            _holdBoundariesRemaining--;
            return;
        }

        // Tier-driven snapshot selection
        _selection = _tier switch
        {
            MusicTier.Ambient => AmbientChoices.Pick(_rng),
            MusicTier.Soft => SoftChoices.Pick(_rng),
            MusicTier.Core => CoreChoices.Pick(_rng),
            MusicTier.Peak => PeakChoices.Pick(_rng),
            _ => default,
        };

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

    private static void ApplyStemSelection(Span<float> v, StemSelection selection)
    {
        foreach (var stem in StemValues)
        {
            if (!selection.Contains(stem))
                continue;

            var level = selection.IsLow(stem) ? Levels.LowLevelFor(stem) : Levels.LevelFor(stem);

            v[(ushort)stem] = level;
        }
    }

    private int NextHold() => _rng.Next(1, 3 + 1); // + 1 converts to exclusive bound

    private static string StemKey(Stems stem) => $"Venezuela/{stem}";

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    internal enum Stems
    {
        BassA,
        BassA2,
        BassB,
        BassPad,
        LeadA,
        LeadA2,
        LeadB,
        PercKickFour,
        PercKickSparse,
        PercHatsBusy,
        PercHatsTight,
        PercSnare,
        PercPulse,
    }

    private static class Levels
    {
        // Min 0.0f, Max 1.0f
        internal static float LevelFor(Stems stem) => stem switch
        {
            Stems.BassPad => 1f,
            _ => 0.8f,
        };

        internal static float LowLevelFor(Stems stem) => stem switch
        {
            Stems.BassA or Stems.BassA2 or Stems.BassB => LevelFor(stem) * 0.7f,
            _ => LevelFor(stem) * 0.5f,
        };
    }
}

internal static class Choose
{
    internal static WeightedChoice Stems() => new();

    extension(WeightedChoice c)
    {
        internal WeightedChoice Mostly(params StemItem[] items) => c.Weight(30, items);
        internal WeightedChoice Sometimes(params StemItem[] items) => c.Weight(10, items);
        internal WeightedChoice Rarely(params StemItem[] items) => c.Weight(4, items);
        internal WeightedChoice VeryRarely(params StemItem[] items) => c.Weight(1, items);
    }
}

internal static class Stem
{
    internal static StemItem Low(Venezuela.Stems stem) => new(stem, true);
}

internal sealed class WeightedChoice
{
    private readonly List<(int Weight, StemSelection Value)> _items = [];

    public WeightedChoice Weight(int weight, params StemItem[] items)
    {
        ulong onMask = 0;
        ulong lowMask = 0;

        foreach (var item in items)
        {
            var bit = 1UL << (int)item.Stem;
            onMask |= bit;
            if (item.Low)
                lowMask |= bit;
        }

        _items.Add((weight, new StemSelection(new StemSet(onMask), new StemSet(lowMask))));
        return this;
    }

    public StemSelection Pick(Random rng)
    {
        var total = 0;
        for (var i = 0; i < _items.Count; i++)
            total += _items[i].Weight;

        var roll = rng.Next(total);
        for (var i = 0; i < _items.Count; i++)
        {
            roll -= _items[i].Weight;
            if (roll < 0)
            {
                var result = _items[i].Value;
                Console.WriteLine("Picked: " + result);
                return result;
            }
        }

        var fallback = _items[^1].Value;
        Console.WriteLine("Picked: " + fallback);
        return fallback;
    }
}

internal readonly record struct StemItem(Venezuela.Stems Stem, bool Low)
{
    public static implicit operator StemItem(Venezuela.Stems stem) => new(stem, false);
}

internal readonly record struct StemSelection(StemSet Stems, StemSet Low)
{
    public bool Contains(Venezuela.Stems stem) => Stems.Contains(stem);
    public bool IsLow(Venezuela.Stems stem) => Low.Contains(stem);
}

internal readonly record struct StemSet(ulong Mask)
{
    public bool Contains(Venezuela.Stems stem) => (Mask & (1UL << (int)stem)) != 0;
}