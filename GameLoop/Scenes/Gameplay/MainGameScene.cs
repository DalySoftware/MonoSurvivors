using System;
using ContentLibrary;
using Gameplay;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Enemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay;

internal class MainGameScene : IScene
{
    private readonly Texture2D _backgroundTile;
    private readonly ContentManager _content;
    private readonly EntityManager _entityManager;
    private readonly GraphicsDevice _graphics;
    private readonly GameplayInputManager _input;
    private readonly SpriteBatch _spriteBatch;

    public MainGameScene(GraphicsDevice graphicsDevice, GameWindow window, ContentManager coreContent, Action exitGame)
    {
        _graphics = graphicsDevice;
        _content = new ContentManager(coreContent.ServiceProvider)
        {
            RootDirectory = coreContent.RootDirectory
        };

        _spriteBatch = new SpriteBatch(graphicsDevice);
        _entityManager = new EntityManager(_content);
        _backgroundTile = _content.Load<Texture2D>(Paths.Images.BackgroundTile);

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
        DrawBackground();

        _spriteBatch.Begin();

        _entityManager.Draw(_spriteBatch);

        _spriteBatch.End();
    }

    private void DrawBackground()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointWrap);
        _spriteBatch.Draw(_backgroundTile, _graphics.PresentationParameters.Bounds,
            _graphics.PresentationParameters.Bounds, Color.White);
        _spriteBatch.End();
    }
}