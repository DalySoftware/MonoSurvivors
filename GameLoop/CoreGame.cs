using GameLoop.Scenes;
using GameLoop.Scenes.Gameplay;
using GameLoop.Scenes.Title;
using Microsoft.Xna.Framework;

namespace GameLoop;

public class CoreGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly SceneManager _sceneManager = new(null);

    public CoreGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        IsMouseVisible = true;

        Window.Title = "Mono Survivors";
    }

    private IScene Scene => _sceneManager.Current!;

    protected override void LoadContent()
    {
        Content.RootDirectory = "ContentLibrary";

        var title = new TitleScreen(GraphicsDevice, Window, Content, StartGame, Exit);
        _sceneManager.Switch(title);

        base.LoadContent();
    }

    private void StartGame() => _sceneManager.Switch(new MainGameScene(GraphicsDevice, Window, Content, Exit));

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
        base.Dispose(disposing);
    }
}