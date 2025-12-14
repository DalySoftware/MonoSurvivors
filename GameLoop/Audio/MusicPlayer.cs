using System;
using System.Threading.Tasks;
using ContentLibrary;
using GameLoop.UserSettings;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace GameLoop.Audio;

public sealed class MusicPlayer : IDisposable
{
    private readonly IDisposable? _optionsChangeListener;
    private readonly SoundEffectInstance _soundEffect;

    private float _baseVolume; // store original volume

    public MusicPlayer(ContentManager content, IOptionsMonitor<AudioSettings> settingsMonitor)
    {
        _soundEffect = content.Load<SoundEffect>(Paths.Music.AppleStrudel).CreateInstance();
        _soundEffect.IsLooped = true;

        // Set initial volume
        UpdateVolume(settingsMonitor.CurrentValue);

        // Subscribe to settings changes
        _optionsChangeListener = settingsMonitor.OnChange(UpdateVolume);
    }

    public void Dispose()
    {
        _optionsChangeListener?.Dispose();
        _soundEffect.Dispose();
    }

    private void UpdateVolume(AudioSettings? settings)
    {
        settings ??= new AudioSettings();
        _baseVolume = settings.MasterVolume * settings.MusicVolume;
        _soundEffect.Volume = _baseVolume;
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
}