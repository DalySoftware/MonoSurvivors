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
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;
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
            builder.RegisterType<PanelRenderer>().SingleInstance();
            builder.RegisterType<ToolTipRenderer>().SingleInstance();
            builder.RegisterType<MusicPlayer>().SingleInstance();

            builder.RegisterType<SceneManager>().SingleInstance();
            builder.RegisterType<SoundEffectPlayer>().As<IAudioPlayer>().SingleInstance();

            builder.RegisterType<TitleInputManager>()
                .WithProperty(i => i.OnStartGame, StartGame)
                .WithProperty(i => i.OnExit, Exit)
                .SingleInstance();

            builder.RegisterType<TitleScreen>().InstancePerDependency();
        });

        ReturnToTitle();

        base.LoadContent();
    }

    private void StartGame()
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        // Dispose previous gameplay scope if restarting
        _gameplayScope?.Dispose();

        _gameplayScope = _contentScope.BeginLifetimeScope(builder =>
        {
            builder.Register(ctx => ctx.Resolve<GraphicsDevice>().Viewport);

            builder.RegisterInstance<Action>(Exit).Named<Action>("exitGame");
            builder.RegisterInstance<Action>(ShowSphereGrid).Named<Action>("openSphereGrid");
            builder.RegisterInstance<Action>(ShowPauseMenu).Named<Action>("openPauseMenu");

            // Gameplay-specific services
            builder.RegisterType<BasicGun>();
            builder.RegisterType<PlayerCharacter>().SingleInstance()
                .WithParameter((pi, _) => pi.Name == "position", (_, _) => new Vector2(0, 0))
                .WithParameter((pi, _) => pi.Name == "onDeath", (_, _) => (Action)ShowGameOver)
                .OnActivated(p => p.Instance.WeaponBelt.AddWeapon(p.Context.Resolve<BasicGun>()));

            builder.RegisterType<EffectManager>().SingleInstance();
            builder.RegisterType<EntityManager>().AsSelf().As<ISpawnEntity>().As<IEntityFinder>().SingleInstance();
            builder.RegisterType<LevelManager>().SingleInstance()
                .WithParameter((pi, _) => pi.Name == "onLevelUp", (_, _) => (Action<int>)OnLevelUp);
            builder.RegisterType<ExperienceSpawner>().SingleInstance();
            builder.RegisterType<EnemySpawner>().SingleInstance();

            builder.RegisterType<EntityRenderer>().SingleInstance();

            builder.Register<ChaseCamera>(ctx =>
            {
                var player = ctx.Resolve<PlayerCharacter>();
                var viewport = ctx.Resolve<Viewport>();
                Vector2 viewportSize = new(viewport.Width, viewport.Height);
                return new ChaseCamera(viewportSize, player);
            }).SingleInstance();

            builder.Register<SphereGrid>(ctx =>
            {
                var player = ctx.Resolve<PlayerCharacter>();
                return GridFactory.Create(player.AddPowerUp);
            }).SingleInstance();

            builder.RegisterType<ExperienceBarRenderer>().SingleInstance();
            builder.Register<ExperienceBar>(ctx =>
            {
                var viewport = ctx.Resolve<Viewport>();
                const float padding = 50f;
                var expBarSize = new Vector2(viewport.Width * 0.7f, 20);
                var expBarCentre = new Vector2(viewport.Bounds.Center.ToVector2().X,
                    viewport.Bounds.Height - expBarSize.Y - padding);
                return ctx.Resolve<ExperienceBarRenderer>().Define(expBarCentre, expBarSize);
            });

            builder.RegisterType<HealthBar>()
                .WithProperty(h => h.Position, new Vector2(10, 10));

            builder.RegisterType<GameplayInputManager>()
                .WithProperty(i => i.OnExit, Exit)
                .WithProperty(i => i.OnOpenSphereGrid, ShowSphereGrid)
                .WithProperty(i => i.OnPause, ShowPauseMenu)
                .SingleInstance();

            // Finally register the scene itself
            builder.RegisterType<MainGameScene>()
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
        var mainScene = _gameplayScope.Resolve<MainGameScene>();
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
        var title = _contentScope.Resolve<TitleScreen>();
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