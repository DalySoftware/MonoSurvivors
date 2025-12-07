using System;
using System.Numerics;
using ContentLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

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

        _titleFont = _content.Load<SpriteFont>(Paths.Fonts.KarmaticArcade.Large);
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

        var windowCentre = _window.Centre;
        var shadowOffset = new Vector2(5f, 5f);
        
        const string line1 = "Mono";
        var line1Centre = _titleFont.MeasureString(line1) / 2;
        var line1Position = windowCentre + new Vector2(0f, -75f); 
        _spriteBatch.DrawString(_titleFont, line1, line1Position + shadowOffset, Color.DarkSlateGray, origin: line1Centre);
        _spriteBatch.DrawString(_titleFont, line1, line1Position, Color.DarkOrange, origin: line1Centre);

        const string line2 = "Survivors";
        var line2Centre = _titleFont.MeasureString(line2) / 2;
        var line2Position = windowCentre + new Vector2(0f, 75f);
        _spriteBatch.DrawString(_titleFont, line2, line2Position + shadowOffset, Color.DarkSlateGray, origin: line2Centre);
        _spriteBatch.DrawString(_titleFont, line2, line2Position, Color.DarkOrange, origin: line2Centre);

        _spriteBatch.End();
    }

    public void Dispose()
    {
        _content.Dispose();
        _spriteBatch.Dispose();
    }
}