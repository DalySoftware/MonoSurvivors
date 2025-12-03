using Characters;
using ContentLibrary;
using Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop;

public class MonoSurvivorsGame : Game
{
    private CharacterManager _characterManager = null!;
    private InputManager _input = null!;

    private Texture2D _logo = null!;
    private SpriteBatch _spriteBatch = null!;

    public MonoSurvivorsGame()
    {
        var graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "ContentLibrary";

        Window.Title = "Mono Survivors";
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;

        IsMouseVisible = true;
    }

    private Vector2 MiddleOfScreen =>
        new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) * 0.5f;

    protected override void Initialize()
    {
        base.Initialize();

        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var player = new PlayerCharacter(MiddleOfScreen);
        _characterManager.Add(player);

        var enemySpawner = new EnemySpawner();
        for (var i = 0; i < 10; i++)
            _characterManager.Add(() => enemySpawner.GetEnemyWithRandomPosition(player));
        _input = new InputManager(player);
    }

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

        _input.Update();
        _characterManager.Update(gameTime);

        base.Update(gameTime);
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        _spriteBatch.Draw(_logo, MiddleOfScreen, origin: _logo.Centre);
        _characterManager.Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}