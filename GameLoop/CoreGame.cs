using System;
using GameLoop.Input;
using GameLoop.Music;
using GameLoop.Scenes;
using GameLoop.Scenes.GameOver;
using GameLoop.Scenes.Gameplay;
using GameLoop.Scenes.SphereGridScene;
using GameLoop.Scenes.Pause;
using GameLoop.Scenes.Title;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Levelling;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering;
using Gameplay.Rendering.Effects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;

namespace GameLoop;

public class CoreGame : Game
{
    private readonly SceneManager _sceneManager = new(null);
    private readonly IServiceProvider _services;
    private LevelManager _levelSystem = null!;
    private MusicPlayer _music = null!;
    private PrimitiveRenderer _primitiveRenderer = null!;
    private SphereGrid _sphereGrid = null!;

    public CoreGame()
    {
        var graphicsManager = new GraphicsDeviceManager(this);
        graphicsManager.PreferredBackBufferWidth = 1920;
        graphicsManager.PreferredBackBufferHeight = 1080;
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
        _music = _services.GetRequiredService<MusicPlayer>();
        _music.PlayBackgroundMusic();

        var player = new PlayerCharacter(Window.Centre, effectManager, audioPlayer, ShowGameOver);
        _levelSystem = new LevelManager(player, OnLevelUp);
        _sphereGrid = GridFactory.Create(player.AddPowerUp);

        _sceneManager.Push(new MainGameScene(GraphicsDevice, Content, Exit, entityManager, audioPlayer, effectManager,
            ShowSphereGrid, ShowPauseMenu, player));
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

    private void OnLevelUp(int levelsGained)
    {
        _sphereGrid.AddSkillPoints(levelsGained);
        if (_sphereGrid.CanUnlockAnything)
            ShowSphereGrid();
    }

    private void ShowSphereGrid()
    {
        void OnClose()
        {
            _sceneManager.Pop();
            _music.RestoreBackgroundMusic();
        }

        var scene = new SphereGridScene(
            GraphicsDevice,
            Content,
            _sphereGrid,
            _primitiveRenderer,
            OnClose,
            Exit);
        _sceneManager.Push(scene);
        _music.DuckBackgroundMusic();
    }

    private void ShowPauseMenu()
    {
        void OnResume()
        {
            _sceneManager.Pop();
        }

        var audioSettings = _services.GetRequiredService<IOptions<AudioSettings>>();
        var configuration = _services.GetRequiredService<IConfiguration>();

        var scene = new PauseMenuScene(
            GraphicsDevice,
            Content,
            OnResume,
            ReturnToTitle,
            audioSettings,
            configuration);
        _sceneManager.Push(scene);
    }

    protected override void Update(GameTime gameTime)
    {
        Scene.Update(gameTime);
        BaseInputManager.GameHasFocus = IsActive;

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