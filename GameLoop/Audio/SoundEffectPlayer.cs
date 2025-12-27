using System;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace GameLoop.Audio;

public class SoundEffectPlayer(
    ContentManager content,
    IOptionsMonitor<AudioSettings> settingsMonitor,
    MusicPlayer musicPlayer)
    : IAudioPlayer
{
    private readonly static SoundEffectTypes[] DucksMusic =
    [
        SoundEffectTypes.BasicShoot, SoundEffectTypes.BouncerShoot, SoundEffectTypes.SniperShoot,
        SoundEffectTypes.ShotgunShoot,
    ];
    private readonly SoundEffectContent _effects = new(content);

    private readonly Random _random = new();

    public void Play(SoundEffectTypes effectType)
    {
        EffectsFor(effectType).PickRandom(_random).Play(effectType, settingsMonitor.CurrentValue);

        // Duck music briefly when firing
        if (DucksMusic.Contains(effectType))
            _ = musicPlayer.DuckFor(TimeSpan.FromMilliseconds(40), 0.5f); // fire-and-forget async
    }

    private SoundEffect[] EffectsFor(SoundEffectTypes effectType) => effectType switch
    {
        SoundEffectTypes.BasicShoot => _effects.Shoot,
        SoundEffectTypes.BouncerShoot => _effects.BouncerShoot,
        SoundEffectTypes.SniperShoot => _effects.SniperShoot,
        SoundEffectTypes.ShotgunShoot => _effects.ShotgunShoot,
        SoundEffectTypes.ExperiencePickup => _effects.ExperienceUp,
        SoundEffectTypes.EnemyDeath => _effects.EnemyDeath,
        SoundEffectTypes.PlayerHurt => _effects.PlayerHurt,
        SoundEffectTypes.Lightning => _effects.Lightning,
        _ => throw new ArgumentException("Unknown sound effect type"),
    };
}

internal static class Extensions
{
    internal static T PickRandom<T>(this T[] array, Random random) => array[random.Next(0, array.Length)];

    internal static void Play(this SoundEffect effect, SoundEffectTypes type, AudioSettings settings)
    {
        var volume = settings.MasterVolume * settings.SoundEffectVolume;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (type)
        {
            case SoundEffectTypes.ExperiencePickup:
                effect.Play(volume * 0.2f, 0f, 0f);
                break;
            case SoundEffectTypes.Lightning:
                effect.Play(volume * 0.15f, 0f, 0f);
                break;
            case SoundEffectTypes.SniperShoot:
                effect.Play(volume, .2f, 0f);
                break;
            default:
                effect.Play(volume, 0f, 0f);
                break;
        }
    }
}