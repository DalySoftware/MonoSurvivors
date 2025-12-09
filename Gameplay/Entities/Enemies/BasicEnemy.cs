using System;
using ContentLibrary;
using Gameplay.Behaviour;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies;

public class BasicEnemy : EnemyBase, IVisual
{
    private readonly FollowEntity _followEntity;

    public BasicEnemy(Vector2 initialPosition, IHasPosition target) : base(initialPosition)
    {
        var speedJitter = Random.Shared.NextSingle() * 0.01f; // This reduces enemies stacking a bit
        _followEntity = new FollowEntity(this, target, 0.1f + speedJitter);
        Health = 20f;
        CollisionRadius = 16f;
        Damage = 1;
    }

    public override float Experience => 3f;
    public string TexturePath => Paths.Images.Enemy;

    public override void Update(GameTime gameTime)
    {
        Velocity = _followEntity.CalculateVelocity();
        base.Update(gameTime);
    }
}