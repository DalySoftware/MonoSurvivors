using System.Collections.Generic;
using Gameplay.Audio;

namespace GameLoop.Audio;

// Pure data: shared by Desktop + Web.
public static class SoundEffectCatalog
{
    public static IReadOnlyDictionary<SoundEffectTypes, string[]> Variants { get; } =
        new Dictionary<SoundEffectTypes, string[]>
        {
            [SoundEffectTypes.BasicShoot] = ["Shoot1", "Shoot2", "Shoot3", "Shoot4", "Shoot5"],
            [SoundEffectTypes.BouncerShoot] = ["BouncerShoot1", "BouncerShoot2", "BouncerShoot3"],
            [SoundEffectTypes.SniperShoot] = ["SniperShoot1", "SniperShoot2", "SniperShoot3"],
            [SoundEffectTypes.ShotgunShoot] = ["ShotgunShoot1", "ShotgunShoot2", "ShotgunShoot3"],

            [SoundEffectTypes.ExperiencePickup] =
                ["ExperienceUp1", "ExperienceUp2", "ExperienceUp3", "ExperienceUp4", "ExperienceUp5"],
            [SoundEffectTypes.LevelUp] = ["LevelUp1", "LevelUp2", "LevelUp3", "LevelUp4"],
            [SoundEffectTypes.EnemyDeath] = ["EnemyDeath1", "EnemyDeath2", "EnemyDeath3"],
            [SoundEffectTypes.EnemyExplode] = ["Explosion1", "Explosion2", "Explosion3", "Explosion4"],
            [SoundEffectTypes.PlayerHurt] = ["PlayerHurt1", "PlayerHurt2", "PlayerHurt3"],
            [SoundEffectTypes.Lightning] = ["Lightning1", "Lightning2", "Lightning3", "Lightning4"],
            [SoundEffectTypes.IceAura] = ["IceDamage1", "IceDamage2", "IceDamage3", "IceDamage4"],
            [SoundEffectTypes.UnlockNode] = ["UnlockNode"],
            [SoundEffectTypes.Crit] = ["Crit1", "Crit2", "Crit3"], // drop Crit3 if you truly don't have it
        };

    // Desktop content pipeline name (no extension)
    public static string DesktopContentName(string baseName) => $@"SoundEffects\{baseName}";

    // Web URL, assumes .wav
    public static string WebUrl(string baseName) => $"soundEffects/{baseName}.wav";
}