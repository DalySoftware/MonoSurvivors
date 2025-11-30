using ContentLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "ContentLibrary";
        
        Window.Title = "Mono Survivors";
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _spriteBatch = new(GraphicsDevice);

        base.Initialize();
    }

    private Texture2D _logo = null!;
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _logo = Content.Load<Texture2D>(Paths.Images.MonoGameLogo);

        // TODO: use this.ContentLibrary to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        
        _spriteBatch.Begin();
        
        var middleOfScreen = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) * 0.5f;
        var middleOfLogo = new Vector2(_logo.Width, _logo.Height) *  0.5f;
        _spriteBatch.Draw(_logo, middleOfScreen, origin: middleOfLogo);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}