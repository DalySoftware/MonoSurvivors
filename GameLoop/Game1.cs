using Characters;
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
    private Texture2D _playerTexture = null!;
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _logo = Content.Load<Texture2D>(Paths.Images.MonoGameLogo);
        _playerTexture = Content.Load<Texture2D>(Paths.Images.Player);

        // TODO: use this.ContentLibrary to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        
        _player.UpdatePosition(gameTime);
        
        base.Update(gameTime);
    }

    private readonly PlayerCharacter _player = new();
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        
        _spriteBatch.Begin();

        var middleOfScreen = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) * 0.5f;
        _spriteBatch.Draw(_logo, middleOfScreen, origin: _logo.Centre);
        
        _spriteBatch.Draw(_playerTexture, _player.Position, origin: _playerTexture.Centre);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}