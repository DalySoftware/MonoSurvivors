using System;
using Entities;
using Entities.Combat.Weapons.Projectile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay;

internal class MainGameScene : IScene
{
    private readonly ContentManager _content;
    private readonly EntityManager _entityManager;
    private readonly GameplayInputManager _input;
    private readonly SpriteBatch _spriteBatch;

    public MainGameScene(GraphicsDevice graphicsDevice, GameWindow window, ContentManager coreContent, Action exitGame)
    {
        _content = new ContentManager(coreContent.ServiceProvider)
        {
            RootDirectory = coreContent.RootDirectory
        };

        _spriteBatch = new SpriteBatch(graphicsDevice);
        _entityManager = new EntityManager(_content);

        var player = new PlayerCharacter(window.Centre);
        _entityManager.Add(player);
        _entityManager.Add(new BasicGun(player, _entityManager));

        var enemySpawner = new EnemySpawner(_entityManager, player)
        {
            SpawnDelay = TimeSpan.FromSeconds(1),
            BatchSize = 1
        };
        _entityManager.Add(enemySpawner);

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