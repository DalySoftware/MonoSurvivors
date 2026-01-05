using System;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace GameLoop.Audio;

public sealed class SoundEffectPlayer : IAudioPlayer, IDisposable
{
    private readonly static SoundEffectTypes[] DucksMusic =
    [
        SoundEffectTypes.BasicShoot,
        SoundEffectTypes.BouncerShoot,
        SoundEffectTypes.SniperShoot,
        SoundEffectTypes.ShotgunShoot,
    ];

    private readonly SoundEffectContent _effects;
    private readonly Random _random = new();
    private readonly ISettingsPersistence _settingsPersistence;
    private readonly MusicPlayer _musicPlayer;

    private AudioSettings _audioSettings;

    public SoundEffectPlayer(
        ContentManager content,
        ISettingsPersistence settingsPersistence,
        MusicPlayer musicPlayer)
    {
        _effects = new SoundEffectContent(content);
        _settingsPersistence = settingsPersistence;
        _musicPlayer = musicPlayer;

        // Initial load
        _audioSettings = settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings);

        // Subscribe to changes
        settingsPersistence.OnChanged -= OnSettingsChanged;
        settingsPersistence.OnChanged += OnSettingsChanged;
    }

    public void Play(SoundEffectTypes effectType)
    {
        EffectsFor(effectType)
            .PickRandom(_random)
            .Play(effectType, _audioSettings);

        // Duck music briefly when firing
        if (DucksMusic.Contains(effectType))
            _ = _musicPlayer.DuckFor(TimeSpan.FromMilliseconds(40), 0.5f);
    }

    public void Dispose() => _settingsPersistence.OnChanged -= OnSettingsChanged;

    private void OnSettingsChanged(Type changedType)
    {
        if (changedType == typeof(AudioSettings))
            _audioSettings = _settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings);
    }

    private SoundEffect[] EffectsFor(SoundEffectTypes effectType) => effectType switch
    {
        SoundEffectTypes.BasicShoot => _effects.Shoot,
        SoundEffectTypes.BouncerShoot => _effects.BouncerShoot,
        SoundEffectTypes.SniperShoot => _effects.SniperShoot,
        SoundEffectTypes.ShotgunShoot => _effects.ShotgunShoot,
        SoundEffectTypes.ExperiencePickup => _effects.ExperienceUp,
        SoundEffectTypes.LevelUp => _effects.LevelUp,
        SoundEffectTypes.EnemyDeath => _effects.EnemyDeath,
        SoundEffectTypes.EnemyExplode => _effects.Explosion,
        SoundEffectTypes.PlayerHurt => _effects.PlayerHurt,
        SoundEffectTypes.Lightning => _effects.Lightning,
        SoundEffectTypes.IceAura => _effects.IceDamage,
        SoundEffectTypes.UnlockNode => _effects.UnlockNode,
        SoundEffectTypes.Crit => _effects.Crit,
        _ => throw new ArgumentOutOfRangeException(nameof(effectType)),
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
            case SoundEffectTypes.IceAura or SoundEffectTypes.LevelUp:
                effect.Play(volume * 0.7f, 0f, 0f);
                break;
            case SoundEffectTypes.Crit:
                effect.Play(volume * 0.5f, 0f, 0f);
                break;
            case SoundEffectTypes.ExperiencePickup or SoundEffectTypes.EnemyExplode:
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