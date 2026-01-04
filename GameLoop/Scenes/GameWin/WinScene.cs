using Autofac;
using ContentLibrary;
using GameLoop.Input;
using Gameplay;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.GameWin;

internal class WinScene(
    ContentManager content,
    SpriteBatch spriteBatch,
    GameWindow window,
    WinSceneInputManager input,
    InputGate inputGate,
    GameInputState inputState)
    : IScene
{
    private readonly SpriteFont _messageFont = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);
    private readonly SpriteFont _titleFont = content.Load<SpriteFont>(Paths.Fonts.KarmaticArcade.Large);

    public void Dispose() => spriteBatch.Dispose();

    public void Update(GameTime gameTime)
    {
        if (inputGate.ShouldProcessInput())
            input.Update();
    }

    public void Draw(GameTime gameTime)
    {
        spriteBatch.Begin(SpriteSortMode.FrontToBack);

        DrawTitle();
        DrawHelpText();

        spriteBatch.End();
    }

    private void DrawTitle()
    {
        const string titleText = "You win!";
        var titleSize = _titleFont.MeasureString(titleText);
        var titlePosition = new Vector2(window.Centre.X - titleSize.X / 2, window.Centre.Y - 100);
        spriteBatch.DrawString(_titleFont, titleText, titlePosition, ColorPalette.Orange);
    }

    private void DrawHelpText()
    {
        var instructionsText = inputState.CurrentInputMethod is InputMethod.KeyboardMouse
            ? "SPACE to Restart | ESC to Exit"
            : "START to Restart | BACK to Exit";
        var instructionsSize = _messageFont.MeasureString(instructionsText);
        var instructionsPosition = new Vector2(window.Centre.X - instructionsSize.X / 2, window.Centre.Y + 50);
        spriteBatch.DrawString(_messageFont, instructionsText, instructionsPosition, ColorPalette.LightGray);
    }

    public static void ConfigureServices(ContainerBuilder builder)
    {
        builder.RegisterType<WinSceneInputManager>();
        builder.RegisterType<WinScene>();
    }
}

internal sealed class WinSceneInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState)
    : SingleActionSceneInputManager(
        globalCommands,
        inputState,
        globalCommands.StartGame);