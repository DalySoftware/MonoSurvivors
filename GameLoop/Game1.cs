using Characters;
using ContentLibrary;
using Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop;

public class Game1 : Game
{
    private SpriteBatch _spriteBatch = null!;
    
    private CharacterManager _characterManager = null!;
    private InputManager _input = null!;

    private Vector2 MiddleOfScreen =>
        new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) * 0.5f;
    

    public Game1()
    {
        var graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "ContentLibrary";
        
        Window.Title = "Mono Survivors";
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        base.Initialize();
        
        _spriteBatch = new(GraphicsDevice);
        
        var player = new PlayerCharacter(MiddleOfScreen);
        _characterManager.Add(player);
        
        var enemySpawner = new EnemySpawner();
        for (var i = 0; i < 10; i++)
            _characterManager.Add(() => enemySpawner.GetEnemyWithRandomPosition(player));
        _input = new(player);
    }

    private Texture2D _logo = null!;
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _logo = Content.Load<Texture2D>(Paths.Images.MonoGameLogo);
        _characterManager = new CharacterManager(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        
        _input.Update();
        _characterManager.Update(gameTime);
        
        base.Update(gameTime);
    }

    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        
        _spriteBatch.Begin();

        _spriteBatch.Draw(_logo, MiddleOfScreen, origin: _logo.Centre);
        _characterManager.Draw(_spriteBatch);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}