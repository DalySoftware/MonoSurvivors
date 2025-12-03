using ContentLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes;

internal class TitleScreen : IScene
{
    private readonly ContentManager _content;
    private readonly SpriteBatch _spriteBatch;

    private readonly SpriteFont _titleFont;
    private readonly GameWindow _window;

    public TitleScreen(GraphicsDevice graphicsDevice, GameWindow window, ContentManager coreContent)
    {
        _window = window;
        _spriteBatch = new SpriteBatch(graphicsDevice);

        _content = new ContentManager(coreContent.ServiceProvider)
        {
            RootDirectory = coreContent.RootDirectory
        };

        _titleFont = _content.Load<SpriteFont>(Paths.Fonts.TerminalGrotequeOpen);
    }

    public void Update(GameTime gameTime) { }

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