using System;
using System.Threading.Tasks;
using GameLoop.Audio;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Microsoft.JSInterop;
using PersistenceJsonContext = GameLoop.Persistence.PersistenceJsonContext;

namespace Veil.Web.PlatformServices.Audio;

public sealed class WebMusicPlayer : IMusicPlayer, IDisposable
{
    private readonly static string BackgroundMusicUrl = MusicCatalog.WebUrl(MusicCatalog.Tracks.Venezuela);

    private readonly IJSRuntime _js;
    private readonly ISettingsPersistence _settingsPersistence;
    private readonly MusicDucker _ducking;

    private bool _started;

    public WebMusicPlayer(IJSInProcessRuntime js, ISettingsPersistence settingsPersistence, MusicDucker ducking)
    {
        _js = js;
        _settingsPersistence = settingsPersistence;
        _ducking = ducking;

        _ducking.OnEffectiveVolumeChanged += ApplyVolume;

        UpdateVolume(settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings));

        settingsPersistence.OnChanged -= OnSettingsChange;
        settingsPersistence.OnChanged += OnSettingsChange;
    }

    public void Dispose()
    {
        _settingsPersistence.OnChanged -= OnSettingsChange;
        _ducking.OnEffectiveVolumeChanged -= ApplyVolume;

        // Best-effort stop (don’t throw during teardown)
        _ = SafeStopAsync();
    }

    public void PlayBackgroundMusic() =>
        // Fire-and-forget; browsers may block autoplay without user gesture.
        _ = StartOrUpdateAsync();

    public async Task DuckFor(TimeSpan? duration = null, float duckFactor = 0.7f)
    {
        duration ??= TimeSpan.FromMilliseconds(100);

        using (_ducking.BeginTemporaryDuck(duckFactor))
        {
            await Task.Delay(duration.Value);
        }
    }

    public void DuckBackgroundMusic() => _ducking.SetManualDuck(0.7f);

    public void RestoreBackgroundMusic() => _ducking.ClearManualDuck();

    private void OnSettingsChange(Type type)
    {
        if (type != typeof(AudioSettings)) return;
        UpdateVolume(_settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings));
    }

    private void UpdateVolume(AudioSettings settings)
    {
        var baseVolume = settings.MasterVolume * settings.MusicVolume;
        _ducking.SetBaseVolume(baseVolume);
    }

    private void ApplyVolume(float volume01)
    {
        // If music not started yet, keep the latest volume and apply after start.
        if (_started)
            _ = _js.InvokeVoidAsync("veilAudio.setMusicVolume", volume01);
    }

    private async Task StartOrUpdateAsync()
    {
        try
        {
            if (!_started)
            {
                // Start looped music and set initial gain.
                await _js.InvokeVoidAsync("veilAudio.startMusic", BackgroundMusicUrl, _ducking.EffectiveVolume);
                _started = true;
                return;
            }

            // Already started: just update volume.
            await _js.InvokeVoidAsync("veilAudio.setMusicVolume", _ducking.EffectiveVolume);
        }
        catch (Exception ex)
        {
            // Don’t silently fail. Browsers can block audio until user gesture.
            Console.Error.WriteLine($"WebMusicPlayer: failed to start/update music. {ex}");
        }
    }

    private async Task SafeStopAsync()
    {
        try
        {
            if (_started)
                await _js.InvokeVoidAsync("veilAudio.stopMusic");
        }
        catch
        {
            // ignore
        }
        finally
        {
            _started = false;
        }
    }
}