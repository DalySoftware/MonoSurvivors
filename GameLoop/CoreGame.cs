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
using Gameplay;
using Gameplay.Audio;
using Gameplay.Rendering;
using Gameplay.Rendering.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop;

public class CoreGame : Game, IGlobalCommands
{
    private readonly SceneManager _sceneManager = new(null);
    private readonly GameContainer _container;

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

    public void ShowGameOver()
    {
        var scope = _gameplayScope.BeginLifetimeScope(GameOverScene.ConfigureServices);

        var gameOverScene = scope.Resolve<GameOverScene>();
        _sceneManager.Push(gameOverScene);
    }

    public void ReturnToTitle()
    {
        var title = _contentScope.Resolve<TitleScene>();
        _sceneManager.Push(title);
    }


    public void ShowSphereGrid()
    {
        var scope = _gameplayScope.BeginLifetimeScope();

        // Resolve the scene from the scope
        var scene = scope.Resolve<SphereGridScene>();
        _sceneManager.Push(scene);

        // Duck the music while the scene is active
        var music = scope.Resolve<MusicPlayer>();
        music.DuckBackgroundMusic();
    }


    public void ShowPauseMenu()
    {
        var scope = _gameplayScope.BeginLifetimeScope(PauseMenuScene.ConfigureServices);

        var scene = scope.Resolve<PauseMenuScene>();
        _sceneManager.Push(scene);
    }

    public void ResumeGame() => _sceneManager.Pop();

    public void StartGame()
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        // Dispose previous gameplay scope if restarting
        _gameplayScope?.Dispose();

        _gameplayScope = _contentScope
            .BeginLifetimeScope(builder =>
            {
                MainGameScene.ConfigureServices(builder);
                SphereGridScene.ConfigureServices(builder); // sphere grid shares lifetime
            });

        // Resolve and push the scene
        var mainScene = _gameplayScope.Resolve<MainGameScene>();
        _sceneManager.Push(mainScene);

        // Play background music
        var music = _gameplayScope.Resolve<MusicPlayer>();
        music.PlayBackgroundMusic();
    }

    public void CloseSphereGrid()
    {
        _sceneManager.Pop();
        _gameplayScope.Resolve<MusicPlayer>().RestoreBackgroundMusic();
    }

    public void ShowMouse() => IsMouseVisible = true;
    public void HideMouse() => IsMouseVisible = false;

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

            builder.RegisterType<TitleInputManager>().SingleInstance();
            builder.RegisterType<TitleScene>().InstancePerDependency();
        });

        ReturnToTitle();

        base.LoadContent();
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
        _container.Root.Dispose();
        Scene.Dispose();
        _sceneManager.Dispose();
        base.Dispose(disposing);
    }
}