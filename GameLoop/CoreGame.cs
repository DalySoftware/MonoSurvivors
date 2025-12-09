using System;
using GameLoop.Scenes;
using GameLoop.Scenes.GameOver;
using GameLoop.Scenes.Gameplay;
using GameLoop.Scenes.SphereGridScene;
using GameLoop.Scenes.Title;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Levelling;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering;
using Gameplay.Rendering.Effects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;

namespace GameLoop;

public class CoreGame : Game
{
    private readonly SceneManager _sceneManager = new(null);
    private readonly IServiceProvider _services;
    private SphereGrid _sphereGrid = null!;
    private LevelManager _levelSystem = null!;
    private PrimitiveRenderer _primitiveRenderer = null!;

    public CoreGame()
    {
        var graphicsManager = new GraphicsDeviceManager(this);
        graphicsManager.PreferredBackBufferWidth = 1280;
        graphicsManager.PreferredBackBufferHeight = 720;
        IsMouseVisible = true;

        Window.Title = "Mono Survivors";

        _services = ServiceConfiguration.ConfigureServices(Content);
    }

    private IScene Scene => _sceneManager.Current!;

    protected override void LoadContent()
    {
        Content.RootDirectory = "ContentLibrary";

        var title = new TitleScreen(GraphicsDevice, Window, Content, StartGame, Exit);
        _sceneManager.Push(title);

        base.LoadContent();
    }

    private void StartGame()
    {
        _primitiveRenderer = new PrimitiveRenderer(GraphicsDevice);
        
        var entityManager = _services.GetRequiredService<EntityManager>();
        var audioPlayer = _services.GetRequiredService<IAudioPlayer>();
        var effectManager = _services.GetRequiredService<EffectManager>();
        
        var player = new PlayerCharacter(Window.Centre, effectManager, audioPlayer, ShowGameOver);
        _levelSystem = new LevelManager(player, OnLevelUp);
        _sphereGrid = SphereGrid.Create(player);

        _sceneManager.Push(new MainGameScene(GraphicsDevice, Content, Exit, entityManager, audioPlayer, effectManager, ShowSphereGrid, player));
    }

    private void ShowGameOver()
    {
        var gameOverScene = new GameOverScene(GraphicsDevice, Window, Content, StartGame, ReturnToTitle);
        _sceneManager.Push(gameOverScene);
    }

    private void ReturnToTitle()
    {
        var title = new TitleScreen(GraphicsDevice, Window, Content, StartGame, Exit);
        _sceneManager.Push(title);
    }

    private void OnLevelUp()
    {
        _sphereGrid.AddSkillPoints(1);
        ShowSphereGrid();
    }

    private void ShowSphereGrid()
    {
        var scene = new SphereGridScene(
            GraphicsDevice,
            Content,
            _sphereGrid,
            _primitiveRenderer,
            _sceneManager.Pop,
            Exit);
        _sceneManager.Push(scene);
    }

    protected override void Update(GameTime gameTime)
    {
        Scene.Update(gameTime);

        base.Update(gameTime);
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        Scene.Draw(gameTime);

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        Scene.Dispose();
        _sceneManager.Dispose();
        base.Dispose(disposing);
    }
}