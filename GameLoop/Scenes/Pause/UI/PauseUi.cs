using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ContentLibrary;
using GameLoop.UI;
using GameLoop.UserSettings;
using Gameplay;
using Gameplay.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Pause.UI;

internal sealed class PauseUi : IUiElement
{
    private readonly SpriteBatch _spriteBatch;
    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly Action _onResume;
    private readonly Action _onReturnToTitle;
    private readonly IConfiguration _configuration;
    private readonly AudioSettings _audioSettings;

    private readonly VerticalStack _mainStack;

    // Track buttons and volume controls for input order
    private readonly List<Button> _menuButtons = [];
    private readonly List<VolumeControl> _volumeControls = [];
    private readonly UiRectangle _viewPortRectangle;

    public PauseUi(
        SpriteBatch spriteBatch,
        Viewport viewport,
        ContentManager content,
        PrimitiveRenderer primitiveRenderer,
        IConfiguration configuration,
        IOptions<AudioSettings> audioSettings,
        IGlobalCommands globalCommands)
    {
        _spriteBatch = spriteBatch;
        _primitiveRenderer = primitiveRenderer;
        _configuration = configuration;
        _audioSettings = audioSettings.Value;
        _onResume = globalCommands.ResumeGame;
        _onReturnToTitle = globalCommands.ReturnToTitle;

        _viewPortRectangle = viewport.UiRectangle();
        _mainStack = new VerticalStack(
            _viewPortRectangle.AnchorForPoint(UiAnchor.TopCenter) + new Vector2(0f, 50f),
            100
        );

        // Title
        _mainStack.AddChild(pos =>
            new Label.Factory(content, Paths.Fonts.BoldPixels.Large, "PAUSED", layerDepth: 0.5f)
                .Create(pos, UiAnchor.TopCenter));

        // Volume controls stack
        _mainStack.AddChild(pos =>
        {
            var offset = new Vector2(-300f, 50f); // We manually roughly centre these on the page
            var volumeStack = new VerticalStack(pos + offset, 20);

            _volumeControls.Add(volumeStack.AddChild(p =>
                new VolumeControl.Factory(content, primitiveRenderer, "Master Volume", GetMasterVolume, SetMasterVolume)
                    .Create(p)
            ));

            _volumeControls.Add(volumeStack.AddChild(p =>
                new VolumeControl.Factory(content, primitiveRenderer, "Music Volume", GetMusicVolume, SetMusicVolume)
                    .Create(p)
            ));

            _volumeControls.Add(volumeStack.AddChild(p =>
                new VolumeControl.Factory(content, primitiveRenderer, "Sound FX Volume", GetSoundEffectVolume,
                        SetSoundEffectVolume)
                    .Create(p)
            ));

            return volumeStack;
        });

        // Menu buttons stack
        _mainStack.AddChild(pos =>
        {
            var menuButtonStack = new VerticalStack(pos, 20);
            menuButtonStack.AddChild(pos1 =>
            {
                var button = new Button.Factory(content, primitiveRenderer, "Resume", OnResume).Create(pos1,
                    UiAnchor.TopCenter);
                _menuButtons.Add(button);
                return button;
            });

            menuButtonStack.AddChild(pos1 =>
            {
                var button = new Button.Factory(content, primitiveRenderer, "Exit to Title", OnExitToTitle).Create(pos1,
                    UiAnchor.TopCenter);
                _menuButtons.Add(button);
                return button;
            });
            return menuButtonStack;
        });
    }


    /// <summary>All Buttons in the correct input order: menu buttons first, then volume control buttons.</summary>
    internal IEnumerable<Button> Buttons
    {
        get
        {
            foreach (var btn in _menuButtons)
                yield return btn;

            foreach (var b in _volumeControls.SelectMany(vc => vc.Buttons))
                yield return b;
        }
    }

    public UiRectangle Rectangle => _mainStack.Rectangle;

    public void Draw(SpriteBatch spriteBatch)
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

        _primitiveRenderer.DrawRectangle(
            _spriteBatch,
            new Rectangle(_viewPortRectangle.TopLeft.ToPoint(), _viewPortRectangle.Size.ToPoint()),
            new Color(0, 0, 0, 180),
            0.3f);

        _mainStack.Draw(_spriteBatch);

        _spriteBatch.End();
    }

    // --- Audio getters/setters ---
    private float GetMasterVolume() => _audioSettings.MasterVolume;
    private float GetMusicVolume() => _audioSettings.MusicVolume;
    private float GetSoundEffectVolume() => _audioSettings.SoundEffectVolume;

    private void SetMasterVolume(float value)
    {
        _audioSettings.MasterVolume = value;
        SaveSettings();
    }

    private void SetMusicVolume(float value)
    {
        _audioSettings.MusicVolume = value;
        SaveSettings();
    }

    private void SetSoundEffectVolume(float value)
    {
        _audioSettings.SoundEffectVolume = value;
        SaveSettings();
    }

    private void SaveSettings()
    {
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
        _onReturnToTitle();
    }
}