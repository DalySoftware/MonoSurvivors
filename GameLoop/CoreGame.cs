using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using GameLoop.Audio;
using GameLoop.DependencyInjection;
using GameLoop.Input;
using GameLoop.Scenes;
using GameLoop.Scenes.GameOver;
using GameLoop.Scenes.Gameplay;
using GameLoop.Scenes.Pause;
using GameLoop.Scenes.SphereGridScene;
using GameLoop.Scenes.Title;
using GameLoop.UI;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Levelling;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering;
using Gameplay.Rendering.Effects;
using Gameplay.Rendering.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop;

public class CoreGame : Game
{
    private readonly SceneManager _sceneManager = new(null);
    private readonly GameContainer _container;

    private HashSet<Node> _lastSeenUnlockables = [];
    private ILifetimeScope _contentScope = null!;
    private ILifetimeScope _gameplayScope = null!;

    public CoreGame()
    {
        var graphicsManager = new GraphicsDeviceManager(this);
        graphicsManager.PreferredBackBufferWidth = 1920;
        graphicsManager.PreferredBackBufferHeight = 1080;
        IsMouseVisible = true;

        Window.Title = "Mono Survivors";

        _container = new GameContainer(this);
    }

    private IScene Scene => _sceneManager.Current!;

    protected override void LoadContent()
    {
        Content.RootDirectory = "ContentLibrary";

        _contentScope = _container.Root.BeginLifetimeScope(builder =>
        {
            builder.RegisterInstance(GraphicsDevice).As<GraphicsDevice>();
            builder.RegisterInstance(Content).As<ContentManager>();
            builder.RegisterType<SpriteBatch>().ExternallyOwned();

            builder.RegisterType<PrimitiveRenderer>().SingleInstance();
            builder.RegisterType<PanelRenderer>();
            builder.RegisterType<ToolTipRenderer>();
            builder.RegisterType<MusicPlayer>().SingleInstance();

            builder.RegisterType<SceneManager>().SingleInstance();
            builder.RegisterType<SoundEffectPlayer>().As<IAudioPlayer>().SingleInstance();
        });

        var title = new TitleScreen(
            GraphicsDevice,
            Window,
            Content,
            StartGame,
            Exit);

        _sceneManager.Push(title);

        base.LoadContent();
    }

    private void StartGame()
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        // Dispose previous gameplay scope if restarting
        _gameplayScope?.Dispose();

        _gameplayScope = _contentScope.BeginLifetimeScope(builder =>
        {
            builder.RegisterInstance<Action>(Exit).Named<Action>("exitGame");
            builder.RegisterInstance<Action>(ShowSphereGrid).Named<Action>("openSphereGrid");
            builder.RegisterInstance<Action>(ShowPauseMenu).Named<Action>("openPauseMenu");

            // Gameplay-specific services
            builder.RegisterType<EffectManager>().SingleInstance();
            builder.RegisterType<EntityManager>().SingleInstance();
            builder.RegisterType<LevelManager>().SingleInstance()
                .WithParameter((pi, _) => pi.Name == "onLevelUp", (_, _) => (Action<int>)OnLevelUp);
            builder.RegisterType<PlayerCharacter>().SingleInstance()
                .WithParameter((pi, _) => pi.Name == "position", (_, _) => new Vector2(0, 0))
                .WithParameter((pi, _) => pi.Name == "onDeath", (_, _) => (Action)ShowGameOver);
            builder.RegisterType<ExperienceSpawner>().SingleInstance();

            // Register dependencies for MainGameScene
            builder.RegisterType<EntityRenderer>().InstancePerDependency();
            builder.RegisterType<PanelRenderer>().InstancePerDependency();
            builder.RegisterType<ExperienceBarRenderer>().InstancePerDependency();

            builder.Register<SphereGrid>(ctx =>
            {
                var player = ctx.Resolve<PlayerCharacter>();
                return GridFactory.Create(player.AddPowerUp);
            }).SingleInstance();

            // Finally register the scene itself
            builder.RegisterType<MainGameScene>()
                .As<IScene>()
                .WithParameter(
                    (pi, _) => pi.Name == "exitGame",
                    (_, _) => (Action)Exit)
                .WithParameter(
                    (pi, _) => pi.Name == "openSphereGrid",
                    (_, _) => (Action)ShowSphereGrid)
                .WithParameter(
                    (pi, _) => pi.Name == "openPauseMenu",
                    (_, _) => (Action)ShowPauseMenu)
                .InstancePerDependency();
        });

        // Resolve and push the scene
        var mainScene = _gameplayScope.Resolve<IScene>();
        _sceneManager.Push(mainScene);

        // Play background music
        var music = _gameplayScope.Resolve<MusicPlayer>();
        music.PlayBackgroundMusic();
    }

    private void ShowGameOver()
    {
        var gameOverScene = new GameOverScene(GraphicsDevice, Window, Content, StartGame, ReturnToTitle);
        _sceneManager.Push(gameOverScene);
        SphereGridInputManager.ResetCamera();
    }

    private void ReturnToTitle()
    {
        var title = new TitleScreen(GraphicsDevice, Window, Content, StartGame, Exit);
        _sceneManager.Push(title);
    }

    private void OnLevelUp(int levelsGained)
    {
        var sphereGrid = _gameplayScope.Resolve<SphereGrid>();

        sphereGrid.AddSkillPoints(levelsGained);
        var unlockables = sphereGrid.Unlockable.ToHashSet();
        var anythingHasChanged = !unlockables.SetEquals(_lastSeenUnlockables);
        if (unlockables.Count > 0 && anythingHasChanged)
            ShowSphereGrid();

        _lastSeenUnlockables = unlockables;
    }

    private void ShowSphereGrid()
    {
        var music = _gameplayScope.Resolve<MusicPlayer>();

        var scope = _gameplayScope.BeginLifetimeScope(builder =>
        {
            builder.RegisterType<SphereGridInputManager>()
                .WithProperty(i => i.OnClose, () =>
                {
                    _sceneManager.Pop();
                    music.RestoreBackgroundMusic();
                })
                .WithProperty(i => i.OnExit, Exit);
            builder.RegisterType<SphereGridUi>();

            // Register the scene itself
            builder.RegisterType<SphereGridScene>();
        });

        // Resolve the scene from the scope
        var scene = scope.Resolve<SphereGridScene>();
        _sceneManager.Push(scene);

        // Duck the music while the scene is active
        music.DuckBackgroundMusic();
    }


    private void ShowPauseMenu()
    {
        var scope = _gameplayScope.BeginLifetimeScope(builder =>
        {
            builder.RegisterType<PauseInputManager>()
                .WithProperty(i => i.OnResume, _sceneManager.Pop)
                .WithProperty(i => i.OnExit, ReturnToTitle);

            builder.RegisterType<PauseUi>()
                .WithParameter((p, _) => p.Name == "onResume", (_, _) => (Action)_sceneManager.Pop)
                .WithParameter((p, _) => p.Name == "onExit", (_, _) => (Action)ReturnToTitle);

            // Register the scene itself
            builder.RegisterType<PauseMenuScene>();
        });

        var scene = scope.Resolve<PauseMenuScene>();
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
        GraphicsDevice.Clear(Color.DarkSlateGray);

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