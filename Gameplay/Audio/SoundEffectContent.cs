using ContentLibrary;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Gameplay.Audio;

// Keeps verbose code out of AudioPlayer
internal class SoundEffectContent(ContentManager content)
{
    internal SoundEffect[] ExperienceUp { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp1),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp2),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp3),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp4),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp5)
    ];

    internal SoundEffect[] Shoot { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot1),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot2),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot3),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot4),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot5)
    ];

    internal SoundEffect[] EnemyDeath { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.EnemyDeath1),
        content.Load<SoundEffect>(Paths.SoundEffects.EnemyDeath2),
        content.Load<SoundEffect>(Paths.SoundEffects.EnemyDeath3)
    ];

    internal SoundEffect[] PlayerHurt { get; } =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.PlayerHurt1),
        content.Load<SoundEffect>(Paths.SoundEffects.PlayerHurt2),
        content.Load<SoundEffect>(Paths.SoundEffects.PlayerHurt3),
    ];
}