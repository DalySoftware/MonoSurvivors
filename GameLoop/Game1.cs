using Characters;
using Characters.Enemy;
using ContentLibrary;
using Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    
    private PlayerCharacter _player = null!;
    private BasicEnemy _enemy = null!;
    private InputManager _input = null!;

    private Vector2 MiddleOfScreen =>
        new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) * 0.5f;
    

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
        base.Initialize();
        
        _spriteBatch = new(GraphicsDevice);
        
        _player = new PlayerCharacter(MiddleOfScreen);
        _enemy = new BasicEnemy(Vector2.Zero, _player);
        _input = new(_player);
    }

    private Texture2D _logo = null!;
    private Texture2D _playerTexture = null!;
    private Texture2D _enemyTexture = null!;
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _logo = Content.Load<Texture2D>(Paths.Images.MonoGameLogo);
        _playerTexture = Content.Load<Texture2D>(Paths.Images.Player);
        _enemyTexture = Content.Load<Texture2D>(Paths.Images.Enemy);

        // TODO: use this.ContentLibrary to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        
        _input.Update();
        _player.UpdatePosition(gameTime);
        _enemy.UpdatePosition(gameTime);
        
        base.Update(gameTime);
    }

    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        
        _spriteBatch.Begin();

        _spriteBatch.Draw(_logo, MiddleOfScreen, origin: _logo.Centre);
        _spriteBatch.Draw(_enemyTexture, _enemy.Position, origin: _enemyTexture.Centre);
        _spriteBatch.Draw(_playerTexture, _player.Position, origin: _playerTexture.Centre);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}