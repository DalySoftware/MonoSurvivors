using System;
using System.Threading.Tasks;
using GameLoop.Audio;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using PersistenceJsonContext = GameLoop.Persistence.PersistenceJsonContext;

namespace Veil.Desktop.PlatformServices.Audio;

internal sealed class MusicPlayer : IDisposable, IMusicPlayer
{
    private readonly MusicDucker _ducking;
    private readonly ISettingsPersistence _settingsPersistence;
    private readonly SoundEffectInstance _instance;

    public MusicPlayer(ContentManager content, MusicDucker ducking, ISettingsPersistence settingsPersistence)
    {
        _ducking = ducking;
        _settingsPersistence = settingsPersistence;

        var path = MusicCatalog.DesktopContentName(MusicCatalog.Tracks.Venezuela);
        _instance = content.Load<SoundEffect>(path).CreateInstance();
        _instance.IsLooped = true;

        _ducking.OnEffectiveVolumeChanged += ApplyVolume;

        // Initial volume from settings
        UpdateVolume(settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings));

        settingsPersistence.OnChanged -= OnSettingsChange;
        settingsPersistence.OnChanged += OnSettingsChange;
    }

    public void Dispose()
    {
        _settingsPersistence.OnChanged -= OnSettingsChange;
        _ducking.OnEffectiveVolumeChanged -= ApplyVolume;
        _instance.Dispose();
    }

    public void PlayBackgroundMusic()
    {
        if (_instance.State != SoundState.Playing)
            _instance.Play();
    }

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

    private void ApplyVolume(float volume) => _instance.Volume = volume;
}