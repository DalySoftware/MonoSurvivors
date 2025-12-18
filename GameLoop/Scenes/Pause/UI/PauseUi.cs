using System;
using System.Globalization;
using ContentLibrary;
using GameLoop.UI;
using GameLoop.UserSettings;
using Gameplay.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Pause.UI;

internal class PauseUi
{
    private readonly SpriteBatch _spriteBatch;
    private readonly Viewport _viewport;
    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly IConfiguration _configuration;
    private readonly AudioSettings _audioSettings;
    private readonly Action _onResume;
    private readonly Action _onExit;

    private readonly VolumeControl[] _volumeControls;
    private readonly Button[] _buttons;
    private readonly SpriteFont _font;
    public PauseUi(SpriteBatch spriteBatch, Viewport viewport, ContentManager content,
        PrimitiveRenderer primitiveRenderer,
        IConfiguration configuration, IOptions<AudioSettings> audioSettings, Action onResume, Action onExit)
    {
        _font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);
        _spriteBatch = spriteBatch;
        _viewport = viewport;
        _primitiveRenderer = primitiveRenderer;
        _configuration = configuration;
        _audioSettings = audioSettings.Value;
        _onResume = onResume;
        _onExit = onExit;

        var screenWidth = viewport.Width;
        var screenHeight = viewport.Height;
        var centerX = screenWidth / 2f;
        var centerY = screenHeight / 2f;

        // Volume controls
        var startY = centerY - 200;
        _volumeControls =
        [
            new VolumeControl(content, primitiveRenderer, new Vector2(centerX - 150, startY),
                "Master Volume", GetMasterVolume, SetMasterVolume),
            new VolumeControl(content, primitiveRenderer, new Vector2(centerX - 150, startY + 120),
                "Music Volume", GetMusicVolume, SetMusicVolume),
            new VolumeControl(content, primitiveRenderer, new Vector2(centerX - 150, startY + 240),
                "Sound FX Volume", GetSoundEffectVolume, SetSoundEffectVolume),
        ];

        _buttons =
        [
            new Button(content, primitiveRenderer, new Vector2(centerX, startY + 360), "Resume", OnResume),

            new Button(content, primitiveRenderer, new Vector2(centerX, startY + 480), "Exit to Title", OnExitToTitle),
        ];
    }

    internal void Update()
    {
        var mouseState = Mouse.GetState();

        foreach (var control in _volumeControls) control.Update(mouseState);
        foreach (var button in _buttons) button.Update(mouseState);
    }

    internal void Draw()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

        // Draw semi-transparent background
        _primitiveRenderer.DrawRectangle(_spriteBatch,
            new Rectangle(0, 0, _viewport.Width, _viewport.Height),
            new Color(0, 0, 0, 180), 0.3f);

        // Draw title
        const string title = "PAUSED";
        var titleSize = _font.MeasureString(title);
        var titlePosition = new Vector2(_viewport.Width / 2f - titleSize.X / 2, 100);
        _spriteBatch.DrawString(_font, title, titlePosition, Color.White, 0f, Vector2.Zero, 1f,
            SpriteEffects.None, 0.5f);


        foreach (var control in _volumeControls) control.Draw(_spriteBatch);

        foreach (var button in _buttons) button.Draw(_spriteBatch);
        _spriteBatch.End();
    }

    private float GetMasterVolume() => _audioSettings.MasterVolume;
    private float GetSoundEffectVolume() => _audioSettings.SoundEffectVolume;
    private float GetMusicVolume() => _audioSettings.MusicVolume;

    private void SetMasterVolume(float value)
    {
        _audioSettings.MasterVolume = value;
        SaveSettings();
    }

    private void SetSoundEffectVolume(float value)
    {
        _audioSettings.SoundEffectVolume = value;
        SaveSettings();
    }

    private void SetMusicVolume(float value)
    {
        _audioSettings.MusicVolume = value;
        SaveSettings();
    }

    private void SaveSettings()
    {
        // Settings are bound to the configuration, so we need to write back through IConfiguration
        _configuration["Audio:MasterVolume"] = _audioSettings.MasterVolume.ToString(CultureInfo.InvariantCulture);
        _configuration["Audio:MusicVolume"] = _audioSettings.MusicVolume.ToString(CultureInfo.InvariantCulture);
        _configuration["Audio:SoundEffectVolume"] =
            _audioSettings.SoundEffectVolume.ToString(CultureInfo.InvariantCulture);
    }

    private void OnResume()
    {
        SaveSettings();
        _onResume();
    }

    private void OnExitToTitle()
    {
        SaveSettings();
        _onExit();
    }
}