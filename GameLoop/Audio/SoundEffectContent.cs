using ContentLibrary;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace GameLoop.Audio;

// Keeps verbose code out of AudioPlayer
internal class SoundEffectContent(ContentManager content)
{
    internal SoundEffect[] ExperienceUp { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp1),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp2),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp3),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp4),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp5),
    ];

    internal SoundEffect[] Shoot { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot1),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot2),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot3),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot4),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot5),
    ];

    internal SoundEffect[] BouncerShoot { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.BouncerShoot1),
        content.Load<SoundEffect>(Paths.SoundEffects.BouncerShoot2),
        content.Load<SoundEffect>(Paths.SoundEffects.BouncerShoot3),
    ];

    internal SoundEffect[] SniperShoot { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.SniperShoot1),
        content.Load<SoundEffect>(Paths.SoundEffects.SniperShoot2),
        content.Load<SoundEffect>(Paths.SoundEffects.SniperShoot3),
    ];

    internal SoundEffect[] ShotgunShoot { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.ShotgunShoot1),
        content.Load<SoundEffect>(Paths.SoundEffects.ShotgunShoot2),
        content.Load<SoundEffect>(Paths.SoundEffects.ShotgunShoot3),
    ];

    internal SoundEffect[] Lightning { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.Lightning1),
        content.Load<SoundEffect>(Paths.SoundEffects.Lightning2),
        content.Load<SoundEffect>(Paths.SoundEffects.Lightning3),
        content.Load<SoundEffect>(Paths.SoundEffects.Lightning4),
    ];

    internal SoundEffect[] IceDamage { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.IceDamage1),
        content.Load<SoundEffect>(Paths.SoundEffects.IceDamage2),
        content.Load<SoundEffect>(Paths.SoundEffects.IceDamage3),
        content.Load<SoundEffect>(Paths.SoundEffects.IceDamage4),
    ];

    internal SoundEffect[] EnemyDeath { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.EnemyDeath1),
        content.Load<SoundEffect>(Paths.SoundEffects.EnemyDeath2),
        content.Load<SoundEffect>(Paths.SoundEffects.EnemyDeath3),
    ];

    internal SoundEffect[] PlayerHurt { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.PlayerHurt1),
        content.Load<SoundEffect>(Paths.SoundEffects.PlayerHurt2),
        content.Load<SoundEffect>(Paths.SoundEffects.PlayerHurt3),
    ];

    public SoundEffect[] Explosion { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.Explosion1),
        content.Load<SoundEffect>(Paths.SoundEffects.Explosion2),
        content.Load<SoundEffect>(Paths.SoundEffects.Explosion3),
        content.Load<SoundEffect>(Paths.SoundEffects.Explosion4),
    ];

    public SoundEffect[] LevelUp { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.LevelUp1),
        content.Load<SoundEffect>(Paths.SoundEffects.LevelUp2),
        content.Load<SoundEffect>(Paths.SoundEffects.LevelUp3),
        content.Load<SoundEffect>(Paths.SoundEffects.LevelUp4),
    ];

    public SoundEffect[] UnlockNode { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.UnlockNode),
    ];

    public SoundEffect[] Crit { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.Crit1),
        content.Load<SoundEffect>(Paths.SoundEffects.Crit2),
        content.Load<SoundEffect>(Paths.SoundEffects.Crit3),
    ];
}