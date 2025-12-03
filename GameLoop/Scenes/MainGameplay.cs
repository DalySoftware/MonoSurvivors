using System;
using Characters;
using Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes;

internal class MainGameplay : IScene
{
    private readonly CharacterManager _characterManager;
    private readonly ContentManager _content;
    private readonly InputManager _input;
    private readonly SpriteBatch _spriteBatch;

    private readonly GameWindow _window;

    public MainGameplay(GraphicsDevice graphicsDevice, GameWindow window, ContentManager coreContent, Action exitGame)
    {
        _window = window;
        _content = new ContentManager(coreContent.ServiceProvider)
        {
            RootDirectory = coreContent.RootDirectory
        };

        _spriteBatch = new SpriteBatch(graphicsDevice);

        _characterManager = new CharacterManager(_content);

        var player = new PlayerCharacter(MiddleOfScreen);
        _characterManager.Add(player);

        var enemySpawner = new EnemySpawner();
        for (var i = 0; i < 10; i++)
            _characterManager.Add(() => enemySpawner.GetEnemyWithRandomPosition(player));
        _input = new InputManager(player)
        {
            OnExit = exitGame
        };

        _spriteBatch = new SpriteBatch(graphicsDevice);
    }

    private Vector2 MiddleOfScreen =>
        new Vector2(_window.ClientBounds.Width, _window.ClientBounds.Height) * 0.5f;

    public void Dispose()
    {
        _content.Dispose();
        _spriteBatch.Dispose();
    }

    public void Update(GameTime gameTime)
    {
        _input.Update();
        _characterManager.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();

        _characterManager.Draw(_spriteBatch);

        _spriteBatch.End();
    }
}