using System;
using ContentLibrary;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Gameplay.Audio;

public class AudioPlayer(ContentManager content) : IAudioPlayer
{
    private readonly SoundEffect[] _experienceUpEffects =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp1),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp2),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp3),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp4),
        content.Load<SoundEffect>(Paths.SoundEffects.ExperienceUp5)
    ];

    private readonly Random _random = new();

    private readonly SoundEffect[] _shootEffects =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot1),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot2),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot3),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot4),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot5)
    ];

    public void Play(SoundEffectTypes effectType) => EffectsFor(effectType).PickRandom(_random).Play(effectType);

    private SoundEffect[] EffectsFor(SoundEffectTypes effectType) => effectType switch
    {
        SoundEffectTypes.Shoot => _shootEffects,
        SoundEffectTypes.ExperiencePickup => _experienceUpEffects,
        _ => throw new ArgumentException("Unknown sound effect type")
    };
}

internal static class Extensions
{
    internal static T PickRandom<T>(this T[] array, Random random) => array[random.Next(0, array.Length)];

    internal static void Play(this SoundEffect effect, SoundEffectTypes type)
    {
        switch (type)
        {
            case SoundEffectTypes.ExperiencePickup:
                effect.Play(0.8f, 0f, 0f);
                return;
            case SoundEffectTypes.Shoot:
            default:
                effect.Play();
                break;
        }
    }
}