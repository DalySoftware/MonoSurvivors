using System;
using System.Threading.Tasks;
using ContentLibrary;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using PersistenceJsonContext = GameLoop.Persistence.PersistenceJsonContext;

namespace Veil.Desktop.PlatformServices.Audio;

internal sealed class MusicPlayer : IDisposable, IMusicPlayer
{
    private readonly ISettingsPersistence _settingsPersistence;
    private readonly SoundEffectInstance _soundEffect;

    private float _baseVolume; // store original volume

    public MusicPlayer(ContentManager content, ISettingsPersistence settingsPersistence)
    {
        _settingsPersistence = settingsPersistence;
        _soundEffect = content.Load<SoundEffect>(Paths.Music.Venezuela).CreateInstance();
        _soundEffect.IsLooped = true;

        // Set initial volume
        UpdateVolume(settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings));

        // Subscribe to settings changes
        settingsPersistence.OnChanged -= OnSettingsChange;
        settingsPersistence.OnChanged += OnSettingsChange;
    }

    public void Dispose()
    {
        _settingsPersistence.OnChanged -= OnSettingsChange;
        _soundEffect.Dispose();
    }

    public void PlayBackgroundMusic() => _soundEffect.Play();

    /// <summary>Reduce music volume temporarily for a ducking effect.</summary>
    public async Task DuckFor(TimeSpan? duration = null, float duckFactor = 0.7f)
    {
        if (!_soundEffect.State.Equals(SoundState.Playing))
            return;

        _soundEffect.Volume = _baseVolume * duckFactor;

        duration ??= TimeSpan.FromMilliseconds(100);
        await Task.Delay(duration.Value);

        // Restore volume
        _soundEffect.Volume = _baseVolume;
    }

    public void DuckBackgroundMusic() => _soundEffect.Volume *= 0.7f;

    public void RestoreBackgroundMusic() => _soundEffect.Volume *= 1.4286f; // reciprocal

    private void OnSettingsChange(Type type)
    {
        if (type != typeof(AudioSettings)) return;
        UpdateVolume(_settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings));
    }

    private void UpdateVolume(AudioSettings settings)
    {
        _baseVolume = settings.MasterVolume * settings.MusicVolume;
        _soundEffect.Volume = _baseVolume;
    }
}