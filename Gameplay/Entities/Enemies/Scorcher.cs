using System.Diagnostics.CodeAnalysis;
using ContentLibrary;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies;

public class Scorcher : EnemyBase, ISpriteVisual
{
    private readonly FollowEntity _followEntity;

    [SetsRequiredMembers]
    public Scorcher(Vector2 position, IHasPosition target) : base(position, 1)
    {
        _followEntity = new FollowEntity(this, target, 0.13f);
        Collider = new RectangleCollider(this, 96f, 96f);
        Health = 12f;
    }

    public override float Experience => 2f;
    public string TexturePath => Paths.Images.Scorcher;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Velocity = _followEntity.CalculateVelocity(NearbyEnemies);
    }
}