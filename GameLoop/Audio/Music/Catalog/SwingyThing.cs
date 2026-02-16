using System;
using System.Diagnostics.CodeAnalysis;

namespace GameLoop.Audio.Music.Catalog;

[SuppressMessage("ReSharper", "RedundantExplicitParamsArrayCreation")]
internal sealed class SwingyThing : StemChoiceModule<SwingyThing.Stems>
{
    private readonly static WeightedChoice<Stems> AmbientChoices =
        Choose.Stems<Stems>()
            .Mostly([Stems.ChordsPad, Stems.BassRoots])
            .Mostly([Stems.ChordsPad, Stems.BassBounce])
            .Sometimes([Stem.Low(Stems.ChordsBeat), Stems.HatsTight, Stems.KickSparse])
            .Rarely([Stems.ChordsTight, Stems.BassRoots]);

    private readonly static WeightedChoice<Stems> SoftChoices =
        Choose.Stems<Stems>()
            .Sometimes([Stems.ChordsTight, Stems.HatsTight, Stems.KickSparse])
            .Sometimes([Stems.ChordsPad, Stems.BassBounce, Stems.HatsTight, Stems.KickSparse])
            .Sometimes([Stems.ChordsBeat, Stems.HatsTight, Stems.KickSparse, Stems.BassRoots])
            .Rarely([Stems.ChordsTight, Stems.BassRoots, Stems.HatsTight]);

    private readonly static WeightedChoice<Stems> CoreChoices =
        Choose.Stems<Stems>()
            .Sometimes([Stems.ChordsBeat, Stems.BassBounce, Stems.KickFour, Stems.HatsTight])
            .Sometimes([Stems.ChordsPad, Stems.LeadPreB, Stems.BassBounce, Stems.KickFour, Stems.HatsTight])
            .Rarely([Stems.ChordsBeat, Stems.BassRoots, Stems.KickSparse, Stems.HatsBusy, Stems.Snare])
            .Rarely([Stems.ChordsTight, Stems.LeadPreB, Stems.BassRoots, Stems.KickSparse, Stems.HatsBusy])
            .Rarely([Stems.ChordsPad, Stems.HatsTight, Stems.LeadPreB]);

    private readonly static WeightedChoice<Stems> PeakChoices =
        Choose.Stems<Stems>()
            .Sometimes([Stems.ChordsTight, Stems.LeadA, Stems.KickFour, Stems.HatsTight])
            .Sometimes([Stems.ChordsTight, Stems.LeadB, Stems.KickFour, Stems.HatsBusy, Stems.Snare])
            .Sometimes(
                [Stems.ChordsBeat, Stems.LeadPreB, Stems.BassBounce, Stems.KickFour, Stems.HatsBusy, Stems.Snare])
            .Rarely([Stems.ChordsPad, Stems.LeadA, Stems.BassRoots, Stems.HatsBusy, Stems.KickSparse, Stems.Snare])
            .Rarely([Stem.Low(Stems.ChordsTight), Stems.LeadPreB, Stems.BassRoots, Stems.HatsTight, Stems.Snare]);

    // 4 bars @ 125 BPM = 7.680s
    public override TimeSpan LoopDuration { get; } = TimeSpan.FromSeconds(7.680);

    protected override string StemKey(Stems stem) => $"SwingyThing/{stem}";

    protected override StemSelection<Stems> PickSelection(MusicTier tier) => tier switch
    {
        MusicTier.Ambient => AmbientChoices.Pick(Random),
        MusicTier.Soft => SoftChoices.Pick(Random),
        MusicTier.Core => CoreChoices.Pick(Random),
        MusicTier.Peak => PeakChoices.Pick(Random),
        _ => default,
    };

    protected override int NextHold() => Tier switch
    {
        // + 1 converts to exclusive bound
        MusicTier.Ambient => Random.Next(4, 8 + 1),
        MusicTier.Peak => Random.Next(1, 3 + 1),
        _ => Random.Next(2, 5 + 1),
    };

    protected override float LevelFor(Stems stem) => 0.8f;
    protected override float LowLevelFor(Stems stem) => LevelFor(stem) * 0.5f;

    internal enum Stems
    {
        BassBounce,
        BassRoots,
        ChordsBeat,
        ChordsPad,
        ChordsTight,
        HatsBusy,
        HatsTight,
        KickFour,
        KickSparse,
        LeadA,
        LeadB,
        LeadPreB,
        Snare,
    }
}