using Gameplay;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Enemy;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Tests;

internal class TestGame : Game
{
    public TestGame()
    {
        GraphicsManager = new GraphicsDeviceManager(this);
    }

    public GraphicsDeviceManager GraphicsManager { get; }
}

/// <summary>
///     Check that entity collision radii are close to rendered size
/// </summary>
public class TextureMapTests
{
    private static TextureMap _textureMap = null!;

    private static Vector2 Zero => Vector2.Zero;

    [Before(Class)]
    public static void Setup()
    {
        using var game = new TestGame();
        game.RunOneFrame();

        var content = new ContentManager(game.Services, "DesktopGL/Content/");
        _textureMap = new TextureMap(content);
    }

    [Test]
    public async Task Player()
    {
        var player = new PlayerCharacter(Zero);
        var texture = _textureMap.TextureFor(player);

        await Assert.That(player.CollisionRadius / texture.Width).IsBetween(0.4f, 0.6f);
    }

    [Test]
    public async Task BasicEnemy()
    {
        var enemy = new BasicEnemy(Zero, new PlayerCharacter(Zero));
        var texture = _textureMap.TextureFor(enemy);

        await Assert.That(enemy.CollisionRadius / texture.Width).IsBetween(0.4f, 0.6f);
    }

    [Test]
    public async Task Bullet()
    {
        var bullet = new Bullet(Zero, Zero);
        var texture = _textureMap.TextureFor(bullet);

        await Assert.That(bullet.CollisionRadius / texture.Width).IsBetween(0.4f, 0.6f);
    }
}