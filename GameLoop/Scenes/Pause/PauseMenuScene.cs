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
    private readonly Action _onExit;
    private readonly Action _onResume;
    private readonly PauseInputManager _input;

    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly SpriteBatch _spriteBatch;
    private readonly List<VolumeControl> _volumeControls = [];

    private MouseState _previousMouseState;

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

        foreach (var control in _volumeControls) control.Update(mouseState, _previousMouseState);

        foreach (var button in _buttons) button.Update(mouseState, _previousMouseState);

        _previousMouseState = mouseState;
    }

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

        // Draw semi-transparent background
        var viewport = _spriteBatch.GraphicsDevice.Viewport;
        _primitiveRenderer.DrawRectangle(_spriteBatch,
            new Rectangle(0, 0, viewport.Width, viewport.Height),
            new Color(0, 0, 0, 180), 0.9f);

        // Draw title
        const string title = "PAUSED";
        var titleSize = _font.MeasureString(title);
        var titlePosition = new Vector2(viewport.Width / 2f - titleSize.X / 2, 100);
        _spriteBatch.DrawString(_font, title, titlePosition, Color.White, 0f, Vector2.Zero, 1f,
            SpriteEffects.None, 0.05f);

        foreach (var control in _volumeControls) control.Draw(_spriteBatch);

        foreach (var button in _buttons) button.Draw(_spriteBatch);

        _spriteBatch.End();
    }

    public void Dispose() => _spriteBatch?.Dispose();

    private void CreateUi(GraphicsDevice graphicsDevice)
    {
        var screenWidth = graphicsDevice.Viewport.Width;
        var screenHeight = graphicsDevice.Viewport.Height;
        var centerX = screenWidth / 2f;
        var centerY = screenHeight / 2f;

        // Volume controls
        var startY = centerY - 150;
        _volumeControls.Add(new VolumeControl(_content, new Vector2(centerX - 300, startY), "Master Volume",
            () => _audioSettings.MasterVolume, v => _audioSettings.MasterVolume = v));

        _volumeControls.Add(new VolumeControl(_content, new Vector2(centerX - 300, startY + 80), "Music Volume",
            () => _audioSettings.MusicVolume, v => _audioSettings.MusicVolume = v));

        _volumeControls.Add(new VolumeControl(_content, new Vector2(centerX - 300, startY + 160), "Sound FX Volume",
            () => _audioSettings.SoundEffectVolume, v => _audioSettings.SoundEffectVolume = v));

        // Buttons
        var resumeButton = new Button(_content, new Vector2(centerX - 150, startY + 260), new Vector2(300, 80),
            "Resume", OnResume);
        _buttons.Add(resumeButton);

        var exitButton = new Button(_content, new Vector2(centerX - 150, startY + 360), new Vector2(300, 80),
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