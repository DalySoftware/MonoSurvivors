using Autofac;
using ContentLibrary;
using GameLoop.Input;
using GameLoop.Persistence;
using GameLoop.Rendering;
using GameLoop.Stats;
using GameLoop.UI;
using Gameplay;
using Gameplay.Rendering.Colors;
using Gameplay.Stats;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.GameWin;

internal class WinScene : IScene
{
    private readonly SpriteBatch _spriteBatch;
    private readonly WinSceneInputManager _input;
    private readonly InputGate _inputGate;
    private readonly GameInputState _inputState;
    private readonly IAppLifeCycle _appLifeCycle;

    private readonly Label _titleLabel;
    private readonly RunStatsPanel _statsPanel;
    private readonly Label _instructionsLabel;
    private InputMethod _lastInputMethod;

    public WinScene(
        RenderScaler renderScaler,
        SpriteBatch spriteBatch,
        WinSceneInputManager input,
        InputGate inputGate,
        GameInputState inputState,
        Label.Factory labelFactory,
        IAppLifeCycle appLifeCycle,
        StatsCounter stats)
    {
        _spriteBatch = spriteBatch;
        _input = input;
        _inputGate = inputGate;
        _inputState = inputState;
        _appLifeCycle = appLifeCycle;

        // Title label
        const string titleText = "You Win!";
        var titleSize = labelFactory.Measure(Paths.Fonts.Righteous.Large, titleText);
        var screen = renderScaler.UiRectangle();
        var titleRectangle = screen.CreateAnchoredRectangle(UiAnchor.TopCenter, titleSize, new Vector2(0f, 50f));

        _titleLabel = labelFactory.Create(
            Paths.Fonts.Righteous.Large,
            titleText,
            titleRectangle.Origin,
            titleRectangle.OriginAnchor,
            ColorPalette.Yellow,
            TextAlignment.Center);

        _statsPanel = new RunStatsPanel(titleRectangle.AnchorForPoint(UiAnchor.BottomCenter), labelFactory, stats);

        // Instructions label
        _lastInputMethod = inputState.CurrentInputMethod;
        _instructionsLabel = labelFactory.Create(
            Paths.Fonts.BoldPixels.Large,
            GetInstructionsText(),
            screen.AnchorForPoint(UiAnchor.BottomCenter) + new Vector2(0f, -150f),
            UiAnchor.TopCenter,
            ColorPalette.LightGray,
            TextAlignment.Center);
    }

    public void Update(GameTime gameTime)
    {
        if (_inputGate.ShouldProcessInput())
            _input.Update();

        // Update instructions text if input method changed
        var currentInput = _inputState.CurrentInputMethod;
        if (currentInput != _lastInputMethod)
        {
            _instructionsLabel.Text = GetInstructionsText();
            _lastInputMethod = currentInput;
        }
    }

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(SpriteSortMode.FrontToBack);

        _titleLabel.Draw(_spriteBatch);
        _statsPanel.Draw(_spriteBatch);
        _instructionsLabel.Draw(_spriteBatch);

        _spriteBatch.End();
    }

    public void Dispose() { }

    private string GetInstructionsText() => _inputState.CurrentInputMethod switch
    {
        InputMethod.KeyboardMouse when _appLifeCycle.CanExit => "[Space] to restart | [Esc] to exit",
        InputMethod.KeyboardMouse => "[Space] to restart",
        InputMethod.Gamepad when _appLifeCycle.CanExit => "[A] to restart | [B] to exit",
        InputMethod.Gamepad => "[A] to restart",
        _ => string.Empty,
    };

    public static void ConfigureServices(ContainerBuilder builder)
    {
        builder.RegisterType<WinSceneInputManager>();
        builder.RegisterType<WinScene>();
    }
}

internal sealed class WinSceneInputManager(
    IGlobalCommands globalCommands,
    IAppLifeCycle appLifeCycle,
    GameInputState inputState,
    ISettingsPersistence settingsPersistence)
    : SingleActionSceneInputManager(
        appLifeCycle,
        inputState,
        settingsPersistence,
        globalCommands.ReturnToTitle);