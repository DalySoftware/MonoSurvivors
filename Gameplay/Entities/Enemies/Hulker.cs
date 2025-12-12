using System.Diagnostics.CodeAnalysis;
using ContentLibrary;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies;

public class Hulker : EnemyBase, ISimpleVisual
{
    private readonly FollowEntity _followEntity;

    [SetsRequiredMembers]
    public Hulker(Vector2 position, IHasPosition target) : base(position, 1)
    {
        _followEntity = new FollowEntity(this, target, 0.07f);
        Collider = new RectangleCollider(this, 128f, 128f);
        Health = 100f;
    }

    public override float Experience => 20f;

    public string TexturePath => Paths.Images.Hulker;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Velocity = _followEntity.CalculateVelocity(NearbyEnemies);
    }
}