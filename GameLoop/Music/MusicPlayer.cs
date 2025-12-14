using System;
using ContentLibrary;
using GameLoop.UserSettings;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace GameLoop.Music;

public sealed class MusicPlayer : IDisposable
{
    private readonly IDisposable? _optionsChangeListener;
    private readonly SoundEffectInstance _soundEffect;

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
        _soundEffect.Volume = settings.MasterVolume * settings.MusicVolume;
    }

    public void PlayBackgroundMusic() => _soundEffect.Play();

    public void DuckBackgroundMusic() => _soundEffect.Volume *= 0.5f;

    public void RestoreBackgroundMusic() => _soundEffect.Volume *= 2f;
}