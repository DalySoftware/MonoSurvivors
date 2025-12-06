using System;
using ContentLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.GameOver;

internal class GameOverScene : IScene
{
    private readonly ContentManager _content;
    private readonly SpriteBatch _spriteBatch;
    private readonly GameOverInputManager _input;
    private readonly SpriteFont _titleFont;
    private readonly SpriteFont _messageFont;
    private readonly GameWindow _window;

    public GameOverScene(
        GraphicsDevice graphicsDevice,
        GameWindow window,
        ContentManager coreContent,
        Action onRestart,
        Action onExit)
    {
        _content = new ContentManager(coreContent.ServiceProvider)
        {
            RootDirectory = coreContent.RootDirectory
        };

        _spriteBatch = new SpriteBatch(graphicsDevice);
        _window = window;

        _titleFont = _content.Load<SpriteFont>(Paths.Fonts.TerminalGrotesqueOpen.Large);
        _messageFont = _content.Load<SpriteFont>(Paths.Fonts.TerminalGrotesqueOpen.Medium);

        _input = new GameOverInputManager
        {
            OnRestart = onRestart,
            OnExit = onExit
        };
    }

    public void Update(GameTime gameTime) => _input.Update();

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();

        // Draw "Game Over" title
        var titleText = "GAME OVER";
        var titleSize = _titleFont.MeasureString(titleText);
        var titlePosition = new Vector2(
            _window.Centre.X - titleSize.X / 2,
            _window.Centre.Y - 100);
        _spriteBatch.DrawString(_titleFont, titleText, titlePosition, Color.OrangeRed);

        // Draw instructions
        var instructionsText = "SPACE to Restart | ESC to Exit";
        var instructionsSize = _messageFont.MeasureString(instructionsText);
        var instructionsPosition = new Vector2(
            _window.Centre.X - instructionsSize.X / 2,
            _window.Centre.Y + 50);
        _spriteBatch.DrawString(_messageFont, instructionsText, instructionsPosition, Color.LightGray);

        _spriteBatch.End();
    }

    public void Dispose()
    {
        _content.Dispose();
        _spriteBatch.Dispose();
    }
}
