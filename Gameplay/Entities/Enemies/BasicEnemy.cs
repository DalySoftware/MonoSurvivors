using System.Diagnostics.CodeAnalysis;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies;

public class BasicEnemy : EnemyBase, ISpriteSheetVisual
{
    private readonly FollowEntity _followEntity;

    [SetsRequiredMembers]
    public BasicEnemy(Vector2 initialPosition, IHasPosition target) : base(initialPosition, 1)
    {
        _followEntity = new FollowEntity(this, target, 0.1f);
        Collider = new CircleCollider(this, 32f);
        Health = 20f;
    }

    public override float Experience => 3f;
    public ISpriteSheet SpriteSheet { get; } = new BasicEnemySpriteSheet();
    public IFrame CurrentFrame => new BasicEnemySpriteSheet.LookDirectionFrame(Velocity);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Velocity = _followEntity.CalculateVelocity(NearbyEnemies);
    }
}