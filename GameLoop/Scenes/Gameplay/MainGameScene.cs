using System;
using ContentLibrary;
using GameLoop.UI;
using Gameplay.Audio;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay;

internal class MainGameScene : IScene
{
    private readonly Texture2D _backgroundTile;
    private readonly ChaseCamera _camera;
    private readonly ContentManager _content;
    private readonly EntityManager _entityManager;
    private readonly EntityRenderer _entityRenderer;
    private readonly HealthBar _healthBar;
    private readonly GameplayInputManager _input;
    private readonly SpriteBatch _spriteBatch;

    public MainGameScene(
        GraphicsDevice graphicsDevice,
        GameWindow window,
        ContentManager coreContent,
        Action exitGame,
        EntityManager entityManager,
        IAudioPlayer audioPlayer)
    {
        _content = new ContentManager(coreContent.ServiceProvider)
        {
            RootDirectory = coreContent.RootDirectory
        };

        _spriteBatch = new SpriteBatch(graphicsDevice);
        _entityManager = entityManager;

        var player = new PlayerCharacter(window.Centre);
        _entityManager.Spawn(player);
        _entityManager.Spawn(new BasicGun(player, _entityManager, _entityManager, audioPlayer));

        Vector2 viewportSize = new(graphicsDevice.PresentationParameters.BackBufferWidth,
            graphicsDevice.PresentationParameters.BackBufferHeight);
        _camera = new ChaseCamera(viewportSize, player);

        _entityRenderer = new EntityRenderer(_content, _spriteBatch, _camera);
        _backgroundTile = _content.Load<Texture2D>(Paths.Images.BackgroundTile);

        var enemySpawner = new EnemySpawner(_entityManager, player, audioPlayer)
        {
            SpawnDelay = TimeSpan.FromSeconds(1),
            BatchSize = 1
        };
        _entityManager.Spawn(enemySpawner);

        _input = new GameplayInputManager(player)
        {
            OnExit = exitGame
        };

        _healthBar = new HealthBar(_content, player)
        {
            Position = new Vector2(10, 10)
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
        _camera.Follow(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        DrawBackground();

        _entityRenderer.Draw(_entityManager.Entities);
        _healthBar.Draw(_spriteBatch);
    }

    private void DrawBackground()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: _camera.Transform);
        _spriteBatch.Draw(_backgroundTile, _camera.VisibleWorldBounds, _camera.VisibleWorldBounds, Color.White);
        _spriteBatch.End();
    }
}