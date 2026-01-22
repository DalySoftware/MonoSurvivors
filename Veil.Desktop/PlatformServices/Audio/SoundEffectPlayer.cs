using System;
using System.Collections.Generic;
using GameLoop.Audio;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Veil.Desktop.PlatformServices.Audio;

public sealed class SoundEffectPlayer : IAudioPlayer, IDisposable
{
    private readonly Dictionary<SoundEffectTypes, SoundEffect[]> _effectsByType;
    private readonly Random _random = new();
    private readonly ISettingsPersistence _settingsPersistence;
    private readonly IMusicPlayer _musicPlayer;

    private AudioSettings _audioSettings;

    public SoundEffectPlayer(
        ContentManager content,
        ISettingsPersistence settingsPersistence,
        IMusicPlayer musicPlayer)
    {
        _settingsPersistence = settingsPersistence;
        _musicPlayer = musicPlayer;

        // Build once: load all variants from the shared catalogue.
        _effectsByType = LoadEffects(content);

        // Initial load
        _audioSettings = settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings);

        // Subscribe to changes
        settingsPersistence.OnChanged -= OnSettingsChanged;
        settingsPersistence.OnChanged += OnSettingsChanged;
    }

    public void Play(SoundEffectTypes effectType)
    {
        _effectsByType[effectType].PickRandom(_random).Play(effectType, _audioSettings);

        // Duck music briefly when firing
        if (SoundEffectTuning.ShouldDuckMusic(effectType))
            _ = _musicPlayer.DuckFor(TimeSpan.FromMilliseconds(40), 0.5f);
    }

    public void Dispose() => _settingsPersistence.OnChanged -= OnSettingsChanged;

    private void OnSettingsChanged(Type changedType)
    {
        if (changedType == typeof(AudioSettings))
            _audioSettings = _settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings);
    }

    private static Dictionary<SoundEffectTypes, SoundEffect[]> LoadEffects(ContentManager content)
    {
        var dict = new Dictionary<SoundEffectTypes, SoundEffect[]>();
        var missing = new List<SoundEffectTypes>();
        var empty = new List<SoundEffectTypes>();

        foreach (var type in Enum.GetValues<SoundEffectTypes>())
        {
            // Optional: skip a sentinel/None value if you have one.
            // if (type == SoundEffectTypes.None) continue;

            if (!SoundEffectCatalog.Variants.TryGetValue(type, out var baseNames))
            {
                missing.Add(type);
                continue;
            }

            if (baseNames.Length == 0)
            {
                empty.Add(type);
                continue;
            }

            var loaded = new SoundEffect[baseNames.Length];
            for (var i = 0; i < baseNames.Length; i++)
            {
                var assetName = SoundEffectCatalog.DesktopContentName(baseNames[i]);
                loaded[i] = content.Load<SoundEffect>(assetName);
            }

            dict[type] = loaded;
        }

        if (missing.Count != 0 || empty.Count != 0)
            throw new InvalidOperationException(
                "SoundEffectCatalog is incomplete.\n" +
                (missing.Count == 0 ? "" : $"Missing entries: {string.Join(", ", missing)}\n") +
                (empty.Count == 0 ? "" : $"Empty variant lists: {string.Join(", ", empty)}\n") +
                "Fix: add/update SoundEffectCatalog.Variants for these SoundEffectTypes.");

        return dict;
    }
}

internal static class Extensions
{
    internal static T PickRandom<T>(this T[] array, Random random) => array[random.Next(0, array.Length)];

    internal static void Play(this SoundEffect effect, SoundEffectTypes type, AudioSettings settings)
    {
        var baseVolume = settings.MasterVolume * settings.SoundEffectVolume;
        var tuning = SoundEffectTuning.Get(type);
        effect.Play(baseVolume * tuning.VolumeMultiplier, tuning.Pitch, 0f);
    }
}