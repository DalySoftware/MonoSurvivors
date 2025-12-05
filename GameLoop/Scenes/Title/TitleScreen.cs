using System;
using ContentLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Title;

internal class TitleScreen : IScene
{
    private readonly ContentManager _content;

    private readonly TitleInputManager _input;
    private readonly SpriteBatch _spriteBatch;

    private readonly SpriteFont _titleFont;
    private readonly GameWindow _window;

    public TitleScreen(GraphicsDevice graphicsDevice, GameWindow window, ContentManager coreContent, Action onStartGame,
        Action onExit)
    {
        _window = window;
        _spriteBatch = new SpriteBatch(graphicsDevice);

        _content = new ContentManager(coreContent.ServiceProvider)
        {
            RootDirectory = coreContent.RootDirectory
        };

        _titleFont = _content.Load<SpriteFont>(Paths.Fonts.TerminalGrotesqueOpen);
        _input = new TitleInputManager
        {
            OnStartGame = onStartGame,
            OnExit = onExit
        };
    }

    public void Update(GameTime gameTime) => _input.Update();

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();

        var titleCentre = _titleFont.MeasureString("MonoSurvivors") / 2;
        var windowCentre = _window.Centre;
        _spriteBatch.DrawString(_titleFont, "MonoSurvivors", windowCentre, Color.OrangeRed, origin: titleCentre);

        _spriteBatch.End();
    }

    public void Dispose()
    {
        _content.Dispose();
        _spriteBatch.Dispose();
    }
}