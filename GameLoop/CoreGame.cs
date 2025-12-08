using System;
using GameLoop.Scenes;
using GameLoop.Scenes.GameOver;
using GameLoop.Scenes.Gameplay;
using GameLoop.Scenes.SphereGridScene;
using GameLoop.Scenes.Title;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Levelling;
using Gameplay.Rendering.Effects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;

namespace GameLoop;

public class CoreGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly SceneManager _sceneManager = new(null);
    private readonly IServiceProvider _services;
    private readonly SphereGrid _sphereGrid = SphereGrid.CreateDemo();
    private LevelManager _levelSystem = null!;

    public CoreGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
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
        var entityManager = _services.GetRequiredService<EntityManager>();
        var audioPlayer = _services.GetRequiredService<IAudioPlayer>();
        var effectManager = _services.GetRequiredService<EffectManager>();
        
        var player = new PlayerCharacter(Window.Centre, effectManager, audioPlayer, ShowGameOver);
        _levelSystem = new LevelManager(player, OnLevelUp);

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
            _sceneManager.Pop);
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