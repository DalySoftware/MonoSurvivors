using System;
using Autofac;
using GameLoop.DependencyInjection;
using GameLoop.Input;
using GameLoop.Rendering;
using GameLoop.Scenes;
using GameLoop.Scenes.GameOver;
using GameLoop.Scenes.Gameplay;
using GameLoop.Scenes.GameWin;
using GameLoop.Scenes.Pause;
using GameLoop.Scenes.SphereGridScene;
using GameLoop.Scenes.Title;
using GameLoop.UI;
using Gameplay;
using Gameplay.Audio;
using Gameplay.Levelling.PowerUps;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop;

public class CoreGame : Game, IGlobalCommands
{
    private readonly static Color BackgroundColor = ColorPalette.Wine.ShiftChroma(-0.04f).ShiftLightness(-0.05f);
    private readonly Action<ContainerBuilder> _configurePlatformServices;
    private readonly GameContainer _container;

    private ILifetimeScope _contentScope = null!;
    private ILifetimeScope _gameplayScope = null!;
    private InputStateManager _inputStateManager = null!;
    private RenderScaler _renderScaler = null!;

    /// <param name="configurePlatformServices">Called during <see cref="LoadContent" /></param>
    public CoreGame(Action<ContainerBuilder>? configurePlatformServices = null)
    {
        _configurePlatformServices = configurePlatformServices ?? (_ => { });
        IsMouseVisible = true;

        Window.Title = "Veil of Cataclysm";

        var graphicsManager = new GraphicsDeviceManager(this);
        graphicsManager.GraphicsProfile = GraphicsProfile.HiDef;
        _container = new GameContainer(this, graphicsManager);
    }

    private SceneManager SceneManager => _contentScope.Resolve<SceneManager>();

    public void ShowGameOver()
    {
        var scope = _gameplayScope.BeginLifetimeScope(GameOverScene.ConfigureServices);

        var gameOverScene = scope.Resolve<GameOverScene>();
        SceneManager.Push(gameOverScene);
    }

    public void ShowWinGame()
    {
        var scope = _gameplayScope.BeginLifetimeScope(WinScene.ConfigureServices);

        var scene = scope.Resolve<WinScene>();
        SceneManager.Push(scene);
    }

    public void ReturnToTitle()
    {
        var title = _contentScope.Resolve<TitleScene>();
        SceneManager.Push(title);
    }

    public void ShowSphereGrid()
    {
        if (SceneManager.Current is SphereGridScene)
            return; // Already open, ignore

        var scope = _gameplayScope.BeginLifetimeScope();

        // Resolve the scene from the scope
        var scene = scope.Resolve<SphereGridScene>();
        SceneManager.Push(scene);

        // Duck the music while the scene is active
        var music = scope.Resolve<IMusicPlayer>();
        music.DuckBackgroundMusic();
    }


    public void ShowPauseMenu()
    {
        var scope = _gameplayScope.BeginLifetimeScope(PauseMenuScene.ConfigureServices);

        var scene = scope.Resolve<PauseMenuScene>();
        SceneManager.Push(scene);
    }

    public void ResumeGame() => SceneManager.Pop();

    public void CloseSphereGrid()
    {
        if (SceneManager.Current is not SphereGridScene)
            return;

        SceneManager.Pop();
        _gameplayScope.Resolve<IMusicPlayer>().RestoreBackgroundMusic();
    }


    public void ShowMouse() => IsMouseVisible = true;
    public void HideMouse() => IsMouseVisible = false;

    public void StartGame(WeaponDescriptor startingWeapon)
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        // Dispose previous gameplay scope if restarting
        _gameplayScope?.Dispose();

        _gameplayScope = _contentScope
            .BeginLifetimeScope(builder =>
            {
                MainGameScene.ConfigureServices(builder);
                SphereGridScene.ConfigureServices(builder); // sphere grid shares lifetime
                builder.RegisterInstance(startingWeapon).Keyed<WeaponDescriptor>("StartingWeapon");
            });

        // Resolve and push the scene
        var mainScene = _gameplayScope.Resolve<MainGameScene>();
        SceneManager.Push(mainScene);

        // Play background music
        var music = _gameplayScope.Resolve<IMusicPlayer>();
        music.PlayBackgroundMusic();
    }

    protected override void LoadContent()
    {
        Content.RootDirectory = "ContentLibrary";

        _contentScope = _container.Root.BeginLifetimeScope(builder =>
        {
            builder.RegisterInstance(Content).As<ContentManager>();

            builder.RegisterType<PrimitiveRenderer>().SingleInstance();
            builder.RegisterType<Panel.Factory>().SingleInstance();
            builder.RegisterType<Button.Factory>().SingleInstance();
            builder.RegisterType<Label.Factory>().SingleInstance();
            builder.RegisterType<ToolTipRenderer>().SingleInstance();
            builder.RegisterType<PowerUpIcons>().SingleInstance();

            TitleScene.ConfigureServices(builder);

            builder.RegisterType<GameInputState>().AsSelf().As<IMouseInputState>().SingleInstance();
            builder.RegisterType<InputStateManager>().SingleInstance();
            builder.RegisterType<InputGate>().SingleInstance();

            _configurePlatformServices(builder);
        });

        _contentScope.Resolve<GameInputState>();
        _inputStateManager = _contentScope.Resolve<InputStateManager>();
        _renderScaler = _contentScope.Resolve<RenderScaler>();
        _contentScope.Resolve<DisplayModeManager>().InitializeDefault();

        ReturnToTitle();

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        SceneManager.Current?.Update(gameTime);
        _inputStateManager.Update(IsActive);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _renderScaler.BeginRenderTarget();
        GraphicsDevice.Clear(BackgroundColor);
        SceneManager.Current?.Draw(gameTime);
        _renderScaler.EndRenderTarget();

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        _container.Root.Dispose();
        base.Dispose(disposing);
    }
}