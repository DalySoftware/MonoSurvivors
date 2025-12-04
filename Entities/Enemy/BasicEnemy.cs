using Entities.Behaviour;

namespace Entities.Enemy;

public class BasicEnemy : EnemyBase
{
    private readonly FollowEntity _followEntity;

    public BasicEnemy(Vector2 initialPosition, IHasPosition target) : base(initialPosition)
    {
        _followEntity = new FollowEntity(this, target, 0.1f);
    }

    public override float Health { get; set; } = 20f;
    public override float CollisionRadius => 16f;
    public override float Damage => 10f;

    public override void Update(GameTime gameTime)
    {
        Velocity = _followEntity.CalculateVelocity();
        base.Update(gameTime);
    }
}