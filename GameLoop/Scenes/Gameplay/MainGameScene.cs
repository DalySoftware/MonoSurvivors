using System;
using ContentLibrary;
using GameLoop.UI;
using Gameplay.Audio;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay;

internal class MainGameScene : IScene
{
    private readonly Texture2D _backgroundTile;
    private readonly ChaseCamera _camera;
    private readonly ContentManager _content;
    private readonly EffectManager _effectManager;
    private readonly EntityManager _entityManager;
    private readonly EntityRenderer _entityRenderer;
    private readonly HealthBar _healthBar;
    private readonly GameplayInputManager _input;
    private readonly SpriteBatch _spriteBatch;
    private readonly ExperienceBar _experienceBar;

    public MainGameScene(
        GraphicsDevice graphicsDevice,
        ContentManager coreContent,
        Action exitGame,
        EntityManager entityManager,
        IAudioPlayer audioPlayer,
        EffectManager effectManager,
        Action openSphereGrid,
        Action openPauseMenu,
        PlayerCharacter player,
        LevelManager levelManager)
    {
        _content = new ContentManager(coreContent.ServiceProvider)
        {
            RootDirectory = coreContent.RootDirectory,
        };

        _spriteBatch = new SpriteBatch(graphicsDevice);
        _entityManager = entityManager;
        _effectManager = effectManager;

        _entityManager.Spawn(player);
        player.WeaponBelt.AddWeapon(new BasicGun(player, _entityManager, _entityManager, audioPlayer));

        Vector2 viewportSize = new(graphicsDevice.PresentationParameters.BackBufferWidth,
            graphicsDevice.PresentationParameters.BackBufferHeight);
        _camera = new ChaseCamera(viewportSize, player);

        _entityRenderer = new EntityRenderer(_content, _spriteBatch, _camera, _effectManager);
        _backgroundTile = _content.Load<Texture2D>(Paths.Images.BackgroundTile);

        var enemySpawner = new EnemySpawner(_entityManager, player, graphicsDevice);

        _entityManager.Spawn(enemySpawner);

        _input = new GameplayInputManager(player)
        {
            OnExit = exitGame,
            OnOpenSphereGrid = openSphereGrid,
            OnPause = openPauseMenu,
        };

        _healthBar = new HealthBar(_content, player)
        {
            Position = new Vector2(10, 10),
        };
        var primitiveRenderer = new PrimitiveRenderer(_content, graphicsDevice);
        var panelRenderer = new PanelRenderer(_content, primitiveRenderer);
        var experienceBarRenderer = new ExperienceBarRenderer(panelRenderer, primitiveRenderer, levelManager);
        const float padding = 50f;
        var expBarSize = new Vector2(graphicsDevice.Viewport.Width * 0.7f, 20);
        var expBarCentre = new Vector2(graphicsDevice.Viewport.Bounds.Center.ToVector2().X,
            graphicsDevice.Viewport.Bounds.Height - expBarSize.Y - padding);
        _experienceBar = experienceBarRenderer.Define(expBarCentre, expBarSize, Layers.UI);
    }

    public void Dispose()
    {
        _content.Dispose();
        _spriteBatch.Dispose();
    }

    public void Update(GameTime gameTime)
    {
        _input.Update();
        _effectManager.Update(gameTime);
        _entityManager.Update(gameTime);
        _camera.Follow(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        DrawBackground();

        _entityRenderer.Draw(_entityManager.Entities);
        _healthBar.Draw(_spriteBatch);
        _experienceBar.Draw(_spriteBatch, Color.CadetBlue, Color.GreenYellow);
    }

    private void DrawBackground()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: _camera.Transform);
        _spriteBatch.Draw(_backgroundTile, _camera.VisibleWorldBounds, _camera.VisibleWorldBounds,
            Color.DarkSlateGray.ShiftChroma(-0.02f).ShiftLightness(0.05f));
        _spriteBatch.End();
    }
}