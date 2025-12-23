using System.Diagnostics.CodeAnalysis;
using ContentLibrary;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies.Types;

public class Scorcher : EnemyBase, ISpriteVisual
{
    private readonly FollowEntity _followEntity;

    [SetsRequiredMembers]
    public Scorcher(Vector2 position, IHasPosition target) : base(position, ScorcherStats())
    {
        _followEntity = new FollowEntity(this, target, 0.13f);
        Collider = new RectangleCollider(this, 96f, 96f);
    }

    public string TexturePath => Paths.Images.Scorcher;

    private static EnemyStats ScorcherStats() => new(12f, 2f, 1);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Velocity = _followEntity.CalculateVelocity(NearbyEnemies);
    }
}