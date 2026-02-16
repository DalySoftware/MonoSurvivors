using System;
using System.Diagnostics.CodeAnalysis;

namespace GameLoop.Audio.Music.Catalog;

[SuppressMessage("ReSharper", "RedundantExplicitParamsArrayCreation")]
internal sealed class Venezuela : StemChoiceModule<Venezuela.Stems>
{
    private readonly static WeightedChoice<Stems> AmbientChoices =
        Choose.Stems<Stems>()
            .Mostly([Stems.BassPad, Stem.Low(Stems.PercPulse)])
            .Sometimes([Stems.BassPad, Stems.PercHatsTight])
            .Sometimes([Stem.Low(Stems.BassA), Stems.PercHatsTight])
            .Sometimes([Stems.BassPad, Stems.PercHatsTight, Stem.Low(Stems.PercPulse)])
            .Rarely([Stem.Low(Stems.PercPulse)]);

    private readonly static WeightedChoice<Stems> SoftChoices =
        Choose.Stems<Stems>()
            .Sometimes([Stems.PercHatsTight, Stem.Low(Stems.BassA)])
            .Sometimes([Stems.PercHatsTight, Stem.Low(Stems.BassA2)])
            .Sometimes([Stems.PercHatsTight, Stem.Low(Stems.BassB)])
            .Sometimes([Stems.PercHatsTight, Stems.PercKickSparse, Stem.Low(Stems.BassA)])
            .Rarely([Stems.PercHatsTight, Stem.Low(Stems.BassA2), Stem.Low(Stems.PercPulse)])
            .Rarely([Stems.BassPad, Stems.PercHatsTight, Stem.Low(Stems.BassA)])
            .VeryRarely([Stems.PercHatsTight, Stems.PercKickSparse, Stem.Low(Stems.BassA), Stems.LeadA2])
            .VeryRarely([Stems.PercHatsTight, Stem.Low(Stems.BassA2), Stems.LeadB]);

    private readonly static WeightedChoice<Stems> CoreChoices =
        Choose.Stems<Stems>()
            .Sometimes([Stems.PercHatsTight, Stems.PercKickFour, Stems.BassA])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA2])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA, Stem.Low(Stems.PercPulse)])
            .Rarely([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB, Stems.PercSnare])
            .Rarely([Stems.BassPad, Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA2])
            .Rarely([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA2, Stem.Low(Stems.PercPulse), Stems.LeadA]);

    private readonly static WeightedChoice<Stems> PeakChoices =
        Choose.Stems<Stems>()
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB, Stems.PercPulse])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA2, Stems.PercPulse])
            .Sometimes([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB, Stems.PercSnare])
            .Rarely([Stems.BassPad, Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassA2, Stems.PercPulse, Stems.LeadB])
            .Rarely([Stems.BassPad, Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB, Stems.PercPulse, Stems.LeadA])
            .VeryRarely([Stems.PercHatsBusy, Stems.PercKickFour, Stems.BassB, Stems.PercPulse, Stems.LeadA2]);

    // 8 bars @ 125 BPM = 15.360s
    public override TimeSpan LoopDuration { get; } = TimeSpan.FromSeconds(15.360);

    protected override StemSelection<Stems> PickSelection(MusicTier tier) => tier switch
    {
        MusicTier.Ambient => AmbientChoices.Pick(Random),
        MusicTier.Soft => SoftChoices.Pick(Random),
        MusicTier.Core => CoreChoices.Pick(Random),
        MusicTier.Peak => PeakChoices.Pick(Random),
        _ => default,
    };

    protected override float LevelFor(Stems stem) => Levels.LevelFor(stem);

    protected override float LowLevelFor(Stems stem) => Levels.LowLevelFor(stem);

    protected override string StemKey(Stems stem) => $"Venezuela/{stem}";

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