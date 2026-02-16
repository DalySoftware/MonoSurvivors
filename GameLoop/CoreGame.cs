using System;
using Autofac;
using GameLoop.Audio.Music;
using GameLoop.Audio.Music.Catalog;
using GameLoop.DependencyInjection;
using GameLoop.Exceptions;
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
using Gameplay.Levelling.PowerUps;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.Effects;
using Gameplay.Rendering.Tooltips;
using Gameplay.Telemetry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop;

public class CoreGame : Game, IGlobalCommands
{
    private readonly static Color BackgroundColor = ColorPalette.Wine.ShiftChroma(-0.04f).ShiftLightness(-0.05f);
    private readonly Action<ContainerBuilder> _configurePlatformServices;
    private readonly GameContainer _container;
    private readonly AsyncPump _asyncPump;

    private ILifetimeScope _contentScope = null!;
    private ILifetimeScope _gameplayScope = null!;
    private InputStateManager _inputStateManager = null!;
    private RenderScaler _renderScaler = null!;
    private IViewportSync _viewportSync = null!;
    private CrtGlitchPulse _crtGlitchPulse = null!;
    private MusicSystem _musicSystem = null!;

    /// <param name="configurePlatformServices">Called during <see cref="LoadContent" /></param>
    public CoreGame(Action<ContainerBuilder>? configurePlatformServices = null)
    {
        _configurePlatformServices = configurePlatformServices ?? (_ => { });
        IsMouseVisible = true;

        Window.Title = "Veil of Cataclysm";

        var graphicsManager = new GraphicsDeviceManager(this);
        graphicsManager.GraphicsProfile = GraphicsProfile.HiDef;
        _container = new GameContainer(this, graphicsManager);
        _asyncPump = _container.Root.Resolve<AsyncPump>();
    }

    private SceneManager SceneManager => _contentScope.Resolve<SceneManager>();

    public void ReturnToTitle()
    {
        var title = _contentScope.Resolve<TitleScene>();
        SceneManager.ClearAndSet(title);
        _musicSystem.SetTier(MusicTier.Ambient);
    }

    public void ShowGameOver()
    {
        var scope = _gameplayScope.BeginLifetimeScope(GameOverScene.ConfigureServices);
        var gameOverScene = scope.Resolve<GameOverScene>();
        SceneManager.ClearAndSet(gameOverScene, scope);
        _musicSystem.SetTier(MusicTier.Ambient);
    }

    public void ShowWinGame()
    {
        var scope = _gameplayScope.BeginLifetimeScope(WinScene.ConfigureServices);
        var winScene = scope.Resolve<WinScene>();
        SceneManager.ClearAndSet(winScene, scope);
        _musicSystem.SetTier(MusicTier.Ambient);
    }

    public void ShowSphereGrid()
    {
        if (SceneManager.Current is SphereGridScene)
            return; // Already open, ignore

        var scope = _gameplayScope.BeginLifetimeScope();

        // Resolve the scene from the scope
        var scene = scope.Resolve<SphereGridScene>();
        SceneManager.Push(scene, scope);
    }


    public void ShowPauseMenu()
    {
        var scope = _gameplayScope.BeginLifetimeScope(PauseMenuScene.ConfigureServices);

        var scene = scope.Resolve<PauseMenuScene>();
        SceneManager.Push(scene, scope);
    }

    public void ResumeGame() => SceneManager.Pop();

    public void CloseSphereGrid()
    {
        if (SceneManager.Current is not SphereGridScene)
            return;

        SceneManager.Pop();
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
        SceneManager.Push(mainScene, _gameplayScope);
        _musicSystem.SetTier(MusicTier.Soft);
    }

    protected override void LoadContent()
    {
        Content.RootDirectory = "ContentLibrary";

        _contentScope = _container.Root.BeginLifetimeScope(builder =>
        {
            builder.RegisterInstance(Content).As<ContentManager>();

            builder.RegisterType<RenderScaler>().AsSelf().As<IRenderViewport>().SingleInstance();
            builder.RegisterType<CrtGlitchPulse>().SingleInstance();

            builder.RegisterType<PrimitiveRenderer>().SingleInstance();
            builder.RegisterType<Panel.Factory>().SingleInstance();
            builder.RegisterType<Button.Factory>().SingleInstance();
            builder.RegisterType<Label.Factory>().SingleInstance();
            builder.RegisterType<ToolTipRenderer>().SingleInstance();
            builder.RegisterType<PowerUpIcons>().SingleInstance();
            builder.RegisterType<PowerUpIconSpriteSheet>().SingleInstance();
            builder.RegisterType<WeaponIconsSpriteSheet>().SingleInstance();

            TitleScene.ConfigureServices(builder);

            builder.RegisterType<GameInputState>().AsSelf().As<IMouseInputState>().SingleInstance();
            builder.RegisterType<InputStateManager>().SingleInstance();
            builder.RegisterType<InputGate>().SingleInstance();

            builder.RegisterType<PerformanceMetrics>().SingleInstance();

            builder.RegisterType<MusicDucker>().SingleInstance();
            builder.RegisterType<SwingyThing>().As<IMusicModule>().SingleInstance();
            builder.RegisterType<Venezuela>().As<IMusicModule>().SingleInstance();
            builder.RegisterType<MusicTierPolicySwitcher>().AsSelf().As<IMusicTierPolicy>().SingleInstance();
            builder.RegisterType<MusicDirector>().SingleInstance();
            builder.RegisterType<MusicTransport>().SingleInstance();
            builder.RegisterType<MusicSystem>().SingleInstance();

            _configurePlatformServices(builder);
        });

        _contentScope.Resolve<GameInputState>();
        _inputStateManager = _contentScope.Resolve<InputStateManager>();
        _renderScaler = _contentScope.Resolve<RenderScaler>();
        _crtGlitchPulse = _contentScope.Resolve<CrtGlitchPulse>();
        _contentScope.Resolve<IDisplayModeManager>().InitializeDefault();
        _viewportSync = _contentScope.Resolve<IViewportSync>();
        _viewportSync.ForceRefresh();
        _musicSystem = _contentScope.Resolve<MusicSystem>();
        _musicSystem.Start();

        ReturnToTitle();

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        _asyncPump.ThrowIfAny();

        _viewportSync.Update();
        SceneManager.Current?.Update(gameTime);
        _inputStateManager.Update(IsActive);
        _crtGlitchPulse.Update(gameTime); // Affects RenderScaler so needs to decay scene independently
        _musicSystem.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _renderScaler.BeginRenderTarget();
        GraphicsDevice.Clear(BackgroundColor);
        SceneManager.Current?.Draw(gameTime);
        _renderScaler.EndRenderTarget(gameTime);

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        _container.Root.Dispose();
        base.Dispose(disposing);
    }
}