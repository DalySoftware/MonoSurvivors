using System;
using Entities;
using Entities.Weapons.Projectile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay;

internal class MainGameScene : IScene
{
    private readonly ContentManager _content;
    private readonly EntityManager _entityManager;
    private readonly BasicGun _gun = null!;
    private readonly GameplayInputManager _input;
    private readonly SpriteBatch _spriteBatch;

    private readonly GameWindow _window;

    public MainGameScene(GraphicsDevice graphicsDevice, GameWindow window, ContentManager coreContent, Action exitGame)
    {
        _window = window;
        _content = new ContentManager(coreContent.ServiceProvider)
        {
            RootDirectory = coreContent.RootDirectory
        };

        _spriteBatch = new SpriteBatch(graphicsDevice);

        _entityManager = new EntityManager(_content);

        var player = new PlayerCharacter(_window.Centre);
        _entityManager.Add(player);

        _gun = new BasicGun(player, _entityManager);
        _entityManager.Add(_gun);

        var enemySpawner = new EnemySpawner();
        for (var i = 0; i < 10; i++)
            _entityManager.Add(() => enemySpawner.GetEnemyWithRandomPosition(player));
        _input = new GameplayInputManager(player)
        {
            OnExit = exitGame
        };

        _spriteBatch = new SpriteBatch(graphicsDevice);
    }


    public void Dispose()
    {
        _content.Dispose();
        _spriteBatch.Dispose();
    }

    public void Update(GameTime gameTime)
    {
        _input.Update();
        _entityManager.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();

        _entityManager.Draw(_spriteBatch);

        _spriteBatch.End();
    }
}