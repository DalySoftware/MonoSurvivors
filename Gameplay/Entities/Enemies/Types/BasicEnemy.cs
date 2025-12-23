using System;
using System.Diagnostics.CodeAnalysis;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies.Types;

public class BasicEnemy : EnemyBase, ISpriteSheetVisual
{
    private readonly FollowEntity _followEntity;

    private TimeSpan _animationCooldown = TimeSpan.Zero;

    [SetsRequiredMembers]
    public BasicEnemy(Vector2 initialPosition, IHasPosition target) : base(initialPosition, BasicEnemyStats())
    {
        _followEntity = new FollowEntity(this, target, 0.07f);
        Collider = new CircleCollider(this, 32f);
    }

    public ISpriteSheet SpriteSheet { get; } = new BasicEnemySpriteSheet();
    public Color? OutlineColor => null;

    public IFrame CurrentFrame { get; private set; } = new BasicEnemySpriteSheet.LookDirectionFrame(Vector2.Zero);

    private static EnemyStats BasicEnemyStats() => new(20f, 1f, 1);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Velocity = _followEntity.CalculateVelocity(NearbyEnemies);

        if (_animationCooldown <= TimeSpan.Zero)
        {
            CurrentFrame = new BasicEnemySpriteSheet.LookDirectionFrame(Velocity);
            _animationCooldown = TimeSpan.FromMilliseconds(200);
        }

        _animationCooldown -= gameTime.ElapsedGameTime;
    }
}