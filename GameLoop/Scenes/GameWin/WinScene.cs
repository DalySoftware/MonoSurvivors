using Autofac;
using ContentLibrary;
using GameLoop.Input;
using GameLoop.Persistence;
using GameLoop.Rendering;
using GameLoop.UI;
using Gameplay;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.GameWin;

internal class WinScene : IScene
{
    private readonly SpriteBatch _spriteBatch;
    private readonly WinSceneInputManager _input;
    private readonly InputGate _inputGate;
    private readonly GameInputState _inputState;

    private readonly Label _titleLabel;
    private readonly Label _instructionsLabel;
    private InputMethod _lastInputMethod;

    public WinScene(
        RenderScaler renderScaler,
        SpriteBatch spriteBatch,
        WinSceneInputManager input,
        InputGate inputGate,
        GameInputState inputState,
        Label.Factory labelFactory)
    {
        _spriteBatch = spriteBatch;
        _input = input;
        _inputGate = inputGate;
        _inputState = inputState;

        // Title label
        const string titleText = "You Win!";
        var titleSize = labelFactory.Measure(Paths.Fonts.Righteous.Large, titleText);
        var titleRectangle = renderScaler.UiRectangle()
            .CreateAnchoredRectangle(UiAnchor.Centre, titleSize, new Vector2(0f, -100f));

        _titleLabel = labelFactory.Create(
            Paths.Fonts.Righteous.Large,
            titleText,
            titleRectangle.Origin,
            titleRectangle.OriginAnchor,
            ColorPalette.Yellow,
            TextAlignment.Center);

        // Instructions label (dynamic)
        _lastInputMethod = inputState.CurrentInputMethod;
        _instructionsLabel = labelFactory.Create(
            Paths.Fonts.BoldPixels.Large,
            GetInstructionsText(),
            titleRectangle.AnchorForPoint(UiAnchor.BottomCenter) + new Vector2(0f, 100f),
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
        _instructionsLabel.Draw(_spriteBatch);

        _spriteBatch.End();
    }

    public void Dispose() { }

    private string GetInstructionsText() =>
        _inputState.CurrentInputMethod is InputMethod.KeyboardMouse
            ? "SPACE to Restart | ESC to Exit"
            : "START to Restart | BACK to Exit";

    public static void ConfigureServices(ContainerBuilder builder)
    {
        builder.RegisterType<WinSceneInputManager>();
        builder.RegisterType<WinScene>();
    }
}

internal sealed class WinSceneInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    ISettingsPersistence settingsPersistence)
    : SingleActionSceneInputManager(
        globalCommands,
        inputState,
        settingsPersistence,
        globalCommands.ReturnToTitle);