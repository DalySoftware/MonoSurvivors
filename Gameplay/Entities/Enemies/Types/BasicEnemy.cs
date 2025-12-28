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
    public BasicEnemy(Vector2 initialPosition, IHasPosition target, bool elite)
        : base(initialPosition, BasicEnemyStats(elite))
    {
        _followEntity = new FollowEntity(this, target, 0.07f);
        Collider = new CircleCollider(this, 32f);
        OutlineColor = elite ? Color.Cyan : null;
    }

    public ISpriteSheet SpriteSheet { get; } = new BasicEnemySpriteSheet();
    public Color? OutlineColor { get; }

    public IFrame CurrentFrame { get; private set; } = new BasicEnemySpriteSheet.LookDirectionFrame(Vector2.Zero);

    private static EnemyStats BasicEnemyStats(bool elite) =>
        elite
            ? new EnemyStats(40f, 2f, 1)
            : new EnemyStats(20f, 1f, 1);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        IntentVelocity = _followEntity.CalculateVelocity(NearbyEnemies);

        if (_animationCooldown <= TimeSpan.Zero)
        {
            CurrentFrame = new BasicEnemySpriteSheet.LookDirectionFrame(Velocity);
            _animationCooldown = TimeSpan.FromMilliseconds(200);
        }

        _animationCooldown -= gameTime.ElapsedGameTime;
    }
}