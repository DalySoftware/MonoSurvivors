using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Gameplay.Audio;

public class AudioPlayer(ContentManager content) : IAudioPlayer
{
    private readonly SoundEffectContent _effects = new(content);
    private readonly Random _random = new();

    public void Play(SoundEffectTypes effectType) => EffectsFor(effectType).PickRandom(_random).Play(effectType);

    private SoundEffect[] EffectsFor(SoundEffectTypes effectType) => effectType switch
    {
        SoundEffectTypes.Shoot => _effects.Shoot,
        SoundEffectTypes.ExperiencePickup => _effects.ExperienceUp,
        SoundEffectTypes.EnemyDeath => _effects.EnemyDeath,
        _ => throw new ArgumentException("Unknown sound effect type")
    };
}

internal static class Extensions
{
    internal static T PickRandom<T>(this T[] array, Random random) => array[random.Next(0, array.Length)];

    internal static void Play(this SoundEffect effect, SoundEffectTypes type)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (type)
        {
            case SoundEffectTypes.ExperiencePickup:
                effect.Play(0.2f, 0f, 0f);
                return;
            default:
                effect.Play();
                break;
        }
    }
}