using GameLoop.Scenes;
using Microsoft.Xna.Framework;

namespace GameLoop;

public class CoreGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private IScene _scene = null!;

    public CoreGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        IsMouseVisible = true;

        Window.Title = "Mono Survivors";
    }

    protected override void LoadContent()
    {
        Content.RootDirectory = "ContentLibrary";

        // _scene = new MainGameplay(GraphicsDevice, Window, Content, Exit);
        _scene = new TitleScreen(GraphicsDevice, Window, Content);

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        _scene.Update(gameTime);

        base.Update(gameTime);
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _scene.Draw(gameTime);

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        _scene.Dispose();
        base.Dispose(disposing);
    }
}