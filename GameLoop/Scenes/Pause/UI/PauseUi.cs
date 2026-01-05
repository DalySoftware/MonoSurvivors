using System;
using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using GameLoop.Persistence;
using GameLoop.UI;
using GameLoop.UserSettings;
using Gameplay;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Pause.UI;

internal sealed class PauseUi : IUiElement, IDisposable
{
    private readonly SpriteBatch _spriteBatch;
    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly ISettingsPersistence _settingsPersistence;

    private AudioSettings _audioSettings;

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
        ISettingsPersistence settingsPersistence,
        IGlobalCommands globalCommands,
        Button.Factory buttonFactory,
        VolumeControl.Factory volumeControlFactory)
    {
        _spriteBatch = spriteBatch;
        _primitiveRenderer = primitiveRenderer;
        _settingsPersistence = settingsPersistence;

        _audioSettings = settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings);
        settingsPersistence.OnChanged += OnSettingsChanged;

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
            var offset = new Vector2(-300f, 50f);
            var volumeStack = new VerticalStack(pos + offset, 20);

            _volumeControls.Add(volumeStack.AddChild(p =>
                volumeControlFactory.Create("Master Volume", p, () => _audioSettings.MasterVolume, SetMasterVolume)));

            _volumeControls.Add(volumeStack.AddChild(p =>
                volumeControlFactory.Create("Music Volume", p, () => _audioSettings.MusicVolume, SetMusicVolume)));

            _volumeControls.Add(volumeStack.AddChild(p =>
                volumeControlFactory.Create("Sound FX Volume", p, () => _audioSettings.SoundEffectVolume,
                    SetSoundEffectVolume)));

            return volumeStack;
        });

        // Menu buttons stack
        _mainStack.AddChild(pos =>
        {
            var menuButtonStack = new VerticalStack(pos, 20);

            menuButtonStack.AddChild(p =>
            {
                var button = buttonFactory.Create("Resume", globalCommands.ResumeGame, p, UiAnchor.TopCenter);
                _menuButtons.Add(button);
                return button;
            });

            menuButtonStack.AddChild(p =>
            {
                var button = buttonFactory.Create("Exit to Title", globalCommands.ReturnToTitle, p, UiAnchor.TopCenter);
                _menuButtons.Add(button);
                return button;
            });

            return menuButtonStack;
        });
    }

    /// <summary>All Buttons in the correct input order.</summary>
    internal IEnumerable<Button> Buttons =>
        _menuButtons.Concat(_volumeControls.SelectMany(vc => vc.Buttons));

    public void Dispose() => _settingsPersistence.OnChanged -= OnSettingsChanged;

    public UiRectangle Rectangle => _mainStack.Rectangle;

    public void Draw(SpriteBatch spriteBatch)
    {
        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            sortMode: SpriteSortMode.FrontToBack);

        _primitiveRenderer.DrawRectangle(
            _spriteBatch,
            new Rectangle(_viewPortRectangle.TopLeft.ToPoint(), _viewPortRectangle.Size.ToPoint()),
            new Color(0, 0, 0, 180),
            0.3f);

        _mainStack.Draw(_spriteBatch);
        _spriteBatch.End();
    }

    private void OnSettingsChanged(Type changedType)
    {
        if (changedType != typeof(AudioSettings))
            return;

        _audioSettings = _settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings);

        foreach (var vc in _volumeControls)
            vc.RefreshLabel();
    }

    // --- Audio setters (persist immediately) ---
    private void SetMasterVolume(float value)
    {
        _audioSettings.MasterVolume = value;
        _settingsPersistence.Save(_audioSettings, PersistenceJsonContext.Default.AudioSettings);
    }

    private void SetMusicVolume(float value)
    {
        _audioSettings.MusicVolume = value;
        _settingsPersistence.Save(_audioSettings, PersistenceJsonContext.Default.AudioSettings);
    }

    private void SetSoundEffectVolume(float value)
    {
        _audioSettings.SoundEffectVolume = value;
        _settingsPersistence.Save(_audioSettings, PersistenceJsonContext.Default.AudioSettings);
    }
}