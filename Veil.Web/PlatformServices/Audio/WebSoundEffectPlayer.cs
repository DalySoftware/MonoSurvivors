using System;
using GameLoop.Audio;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Microsoft.JSInterop;

namespace Veil.Web.PlatformServices.Audio;

public sealed class WebSoundEffectPlayer : IAudioPlayer, IDisposable
{
    private readonly IJSRuntime _js;
    private readonly ISettingsPersistence _settingsPersistence;
    private readonly Random _random = new();

    private AudioSettings _audioSettings;

    public WebSoundEffectPlayer(IJSInProcessRuntime jsRuntime, ISettingsPersistence settingsPersistence)
    {
        _js = jsRuntime;
        _settingsPersistence = settingsPersistence;

        _audioSettings = settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings);

        settingsPersistence.OnChanged -= OnSettingsChanged;
        settingsPersistence.OnChanged += OnSettingsChanged;
    }

    public void Play(SoundEffectTypes effectType)
    {
        if (!SoundEffectCatalog.Variants.TryGetValue(effectType, out var variants) || variants.Length == 0)
            return;

        var baseName = variants[_random.Next(variants.Length)];
        var url = SoundEffectCatalog.WebUrl(baseName);

        var baseVolume = _audioSettings.MasterVolume * _audioSettings.SoundEffectVolume;
        var tuning = SoundEffectTuning.Get(effectType);

        var volume = baseVolume * tuning.VolumeMultiplier;
        var pitch = tuning.Pitch;

        _ = _js.InvokeVoidAsync("veilAudio.playSfx", url, volume, pitch);
    }

    public void Dispose() => _settingsPersistence.OnChanged -= OnSettingsChanged;

    private void OnSettingsChanged(Type t)
    {
        if (t == typeof(AudioSettings))
            _audioSettings = _settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings);
    }
}