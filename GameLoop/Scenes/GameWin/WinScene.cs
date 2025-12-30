using Autofac;
using ContentLibrary;
using GameLoop.Input;
using Gameplay;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.GameWin;

internal class WinScene(
    ContentManager content,
    SpriteBatch spriteBatch,
    GameWindow window,
    WinSceneInputManager input)
    : IScene
{
    private readonly SpriteFont _messageFont = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);
    private readonly SpriteFont _titleFont = content.Load<SpriteFont>(Paths.Fonts.KarmaticArcade.Large);

    public void Dispose() => spriteBatch.Dispose();

    public void Update(GameTime gameTime) => input.Update();

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
        const string instructionsText = "SPACE to play again | ESC to Exit";
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

internal class WinSceneInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    SceneManager sceneManager)
    : BaseInputManager(globalCommands, inputState, sceneManager)
{
    internal void Update()
    {
        if (ShouldSkipInput()) return;

        if (WasPressedThisFrame(Keys.Escape) || WasPressedThisFrame(Buttons.Back)) GlobalCommands.Exit();
        if (InputState.KeyboardState.IsKeyDown(Keys.Space) || WasPressedThisFrame(Buttons.Start))
            GlobalCommands.StartGame();
    }
}