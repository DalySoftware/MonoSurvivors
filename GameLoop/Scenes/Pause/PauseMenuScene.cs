using System;
using System.Collections.Generic;
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

namespace GameLoop.Scenes.Pause;

internal class PauseMenuScene : IScene
{
    private readonly AudioSettings _audioSettings;
    private readonly List<Button> _buttons = [];
    private readonly IConfiguration _configuration;
    private readonly ContentManager _content;
    private readonly SpriteFont _font;
    private readonly PauseInputManager _input;
    private readonly Action _onExit;
    private readonly Action _onResume;

    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly SpriteBatch _spriteBatch;
    private readonly List<VolumeControl> _volumeControls = [];

    public PauseMenuScene(
        GraphicsDevice graphicsDevice,
        ContentManager content,
        Action onResume,
        Action onExit,
        IOptions<AudioSettings> audioSettings,
        IConfiguration configuration)
    {
        _spriteBatch = new SpriteBatch(graphicsDevice);
        _content = content;
        _onResume = onResume;
        _onExit = onExit;
        _audioSettings = audioSettings.Value;
        _configuration = configuration;
        _primitiveRenderer = new PrimitiveRenderer(graphicsDevice);

        _font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);

        _input = new PauseInputManager
        {
            OnExit = onExit,
            OnResume = OnResume
        };

        CreateUi(graphicsDevice);
    }

    public void Update(GameTime gameTime)
    {
        _input.Update();

        var mouseState = Mouse.GetState();

        foreach (var control in _volumeControls) control.Update(mouseState);

        foreach (var button in _buttons) button.Update(mouseState);
    }

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

        var viewport = _spriteBatch.GraphicsDevice.Viewport;
        // Draw semi-transparent background
        _primitiveRenderer.DrawRectangle(_spriteBatch,
            new Rectangle(0, 0, viewport.Width, viewport.Height),
            new Color(0, 0, 0, 180), 0.3f);

        // Draw title
        const string title = "PAUSED";
        var titleSize = _font.MeasureString(title);
        var titlePosition = new Vector2(viewport.Width / 2f - titleSize.X / 2, 100);
        _spriteBatch.DrawString(_font, title, titlePosition, Color.White, 0f, Vector2.Zero, 1f,
            SpriteEffects.None, 0.5f);

        foreach (var control in _volumeControls) control.Draw(_spriteBatch);

        foreach (var button in _buttons) button.Draw(_spriteBatch);

        _spriteBatch.End();
    }


    public void Dispose() => _spriteBatch.Dispose();

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

    private void CreateUi(GraphicsDevice graphicsDevice)
    {
        var screenWidth = graphicsDevice.Viewport.Width;
        var screenHeight = graphicsDevice.Viewport.Height;
        var centerX = screenWidth / 2f;
        var centerY = screenHeight / 2f;

        // Volume controls
        var startY = centerY - 200;
        _volumeControls.Add(new VolumeControl(_content, new Vector2(centerX - 150, startY),
            "Master Volume", GetMasterVolume, SetMasterVolume));

        _volumeControls.Add(new VolumeControl(_content, new Vector2(centerX - 150, startY + 120),
            "Music Volume", GetMusicVolume, SetMusicVolume));

        _volumeControls.Add(new VolumeControl(_content, new Vector2(centerX - 150, startY + 240),
            "Sound FX Volume", GetSoundEffectVolume, SetSoundEffectVolume));

        // Buttons
        var resumeButton = new Button(_content, new Vector2(centerX, startY + 360),
            "Resume", OnResume);
        _buttons.Add(resumeButton);

        var exitButton = new Button(_content, new Vector2(centerX, startY + 480),
            "Exit to Title", OnExitToTitle);
        _buttons.Add(exitButton);
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

    private void SaveSettings()
    {
        // Settings are bound to the configuration, so we need to write back through IConfiguration
        _configuration["Audio:MasterVolume"] = _audioSettings.MasterVolume.ToString(CultureInfo.InvariantCulture);
        _configuration["Audio:MusicVolume"] = _audioSettings.MusicVolume.ToString(CultureInfo.InvariantCulture);
        _configuration["Audio:SoundEffectVolume"] =
            _audioSettings.SoundEffectVolume.ToString(CultureInfo.InvariantCulture);
    }
}