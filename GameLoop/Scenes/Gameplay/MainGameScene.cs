using System;
using ContentLibrary;
using GameLoop.UI;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;
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
    private readonly EffectManager _effectManager;
    private readonly EntityManager _entityManager;
    private readonly EntityRenderer _entityRenderer;
    private readonly HealthBar _healthBar;
    private readonly GameplayInputManager _input;
    private readonly SpriteBatch _spriteBatch;
    private readonly ExperienceBar _experienceBar;

    public MainGameScene(
        SpriteBatch spriteBatch,
        Action exitGame,
        EntityManager entityManager,
        EffectManager effectManager,
        Action openSphereGrid,
        Action openPauseMenu,
        PlayerCharacter player,
        EnemySpawner spawner,
        ChaseCamera camera,
        ContentManager content,
        ExperienceBar experienceBar,
        EntityRenderer entityRenderer)
    {
        _spriteBatch = spriteBatch;
        _entityManager = entityManager;
        _effectManager = effectManager;
        _experienceBar = experienceBar;
        _camera = camera;
        _entityRenderer = entityRenderer;

        _backgroundTile = content.Load<Texture2D>(Paths.Images.BackgroundTile);

        _entityManager.Spawn(player);
        _entityManager.Spawn(spawner);

        _input = new GameplayInputManager(player)
        {
            OnExit = exitGame,
            OnOpenSphereGrid = openSphereGrid,
            OnPause = openPauseMenu,
        };

        _healthBar = new HealthBar(content, player)
        {
            Position = new Vector2(10, 10),
        };
    }

    public void Dispose() { }

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
        _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: _camera.Transform,
            sortMode: SpriteSortMode.FrontToBack);
        _spriteBatch.Draw(_backgroundTile, _camera.VisibleWorldBounds, _camera.VisibleWorldBounds,
            Color.DarkSlateGray.ShiftChroma(-0.02f).ShiftLightness(0.05f));
        _spriteBatch.End();
    }
}