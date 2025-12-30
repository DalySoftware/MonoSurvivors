using Autofac;
using ContentLibrary;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.GameOver;

internal class GameOverScene(
    ContentManager content,
    SpriteBatch spriteBatch,
    GameWindow window,
    GameOverInputManager input)
    : IScene
{
    private readonly SpriteFont _messageFont = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);
    private readonly SpriteFont _titleFont = content.Load<SpriteFont>(Paths.Fonts.KarmaticArcade.Large);

    public void Update(GameTime gameTime) => input.Update();

    public void Draw(GameTime gameTime)
    {
        spriteBatch.Begin(SpriteSortMode.FrontToBack);

        // Draw "Game Over" title
        const string titleText = "Game Over";
        var titleSize = _titleFont.MeasureString(titleText);
        var titlePosition = new Vector2(
            window.Centre.X - titleSize.X / 2,
            window.Centre.Y - 100);
        spriteBatch.DrawString(_titleFont, titleText, titlePosition, Color.Firebrick);

        // Draw instructions
        const string instructionsText = "SPACE to Restart | ESC to Exit";
        var instructionsSize = _messageFont.MeasureString(instructionsText);
        var instructionsPosition = new Vector2(
            window.Centre.X - instructionsSize.X / 2,
            window.Centre.Y + 50);
        spriteBatch.DrawString(_messageFont, instructionsText, instructionsPosition, Color.LightGray);

        spriteBatch.End();
    }

    public void Dispose() => spriteBatch.Dispose();

    public static void ConfigureServices(ContainerBuilder builder)
    {
        builder.RegisterType<GameOverInputManager>();
        builder.RegisterType<GameOverScene>();
    }
}