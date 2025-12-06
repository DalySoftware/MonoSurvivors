using System;
using ContentLibrary;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Gameplay.Audio;

public class AudioPlayer(ContentManager content) : IAudioPlayer
{
    private readonly Random _random = new();

    private readonly SoundEffect[] _shootEffects =
    [
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot1),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot2),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot3),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot4),
        content.Load<SoundEffect>(Paths.SoundEffects.Shoot5)
    ];

    public void Play(SoundEffectTypes effectType) => EffectsFor(effectType).PickRandom(_random).Play();

    private SoundEffect[] EffectsFor(SoundEffectTypes effectType) => effectType switch
    {
        SoundEffectTypes.Shoot => _shootEffects,
        _ => throw new ArgumentException("Unknown sound effect type")
    };
}

internal static class RandomExtensions
{
    internal static T PickRandom<T>(this T[] array, Random random) => array[random.Next(0, array.Length)];
}