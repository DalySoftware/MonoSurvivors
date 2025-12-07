using Gameplay.CollisionDetection;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TUnit.Core.Exceptions;

namespace Tests;

internal class TestGame : Game
{
    public TestGame()
    {
        _ = new GraphicsDeviceManager(this);
        Content.RootDirectory = "DesktopGL/Content/";
    }
}

/// <summary>
///     Check that entity collision radii are close to rendered size
/// </summary>
[NotInParallel] // We can't run the game multiple times at once
public sealed class CollisionRadiusTests : IDisposable
{
    private readonly TestGame _game;

    public CollisionRadiusTests()
    {
        _game = new TestGame();
        _game.RunOneFrame();
    }

    private static Vector2 Zero => Vector2.Zero;

    public void Dispose() => _game.Dispose();

    [Test]
    [MethodDataSource<TestData>(nameof(TestData.Entities))]
    internal async Task CollisionRadiusIsRoughlyEqualToTextureSize(ICircleCollider collider)
    {
        if (collider is not IVisual visual)
            throw new FailTestException("Bad test data: collider is not visual");

        var texture = _game.Content.Load<Texture2D>(visual.TexturePath);

        await Assert.That(collider.CollisionRadius / texture.Width).IsBetween(0.4f, 0.6f);
    }

    internal class TestData
    {
        private static PlayerCharacter Player() => new(Zero, null!);

        public static IEnumerable<Func<ICircleCollider>> Entities() =>
        [
            Player,
            () => new Bullet(Zero, Zero),
            () => new BasicEnemy(Zero, Player()),
            () => new Experience(Zero, 0f, Player(), null!)
        ];
    }
}