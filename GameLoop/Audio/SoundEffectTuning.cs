using System.Collections.Generic;
using Gameplay.Audio;

namespace GameLoop.Audio;

public readonly record struct SfxTuning(float VolumeMultiplier = 1f, float Pitch = 0f)
{
    internal readonly static SfxTuning Default = new(1f);
}

public static class SoundEffectTuning
{
    private readonly static HashSet<SoundEffectTypes> DucksMusic =
    [
        SoundEffectTypes.BasicShoot,
        SoundEffectTypes.BouncerShoot,
        SoundEffectTypes.SniperShoot,
        SoundEffectTypes.ShotgunShoot,
    ];

    public static bool ShouldDuckMusic(SoundEffectTypes type) => DucksMusic.Contains(type);

    public static SfxTuning Get(SoundEffectTypes type) => type switch
    {
        SoundEffectTypes.IceAura or SoundEffectTypes.LevelUp => new SfxTuning(0.7f),
        SoundEffectTypes.Crit => new SfxTuning(0.5f),
        SoundEffectTypes.ExperiencePickup or SoundEffectTypes.EnemyExplode => new SfxTuning(0.2f),
        SoundEffectTypes.Lightning => new SfxTuning(0.15f),
        SoundEffectTypes.SniperShoot => new SfxTuning(1.0f, 0.2f),
        _ => SfxTuning.Default,
    };
}