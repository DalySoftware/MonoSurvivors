using System;
using System.Diagnostics.CodeAnalysis;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies;

public class Hulker : EnemyBase, ISpriteSheetVisual
{
    private readonly FollowEntity _followEntity;
    private TimeSpan _animationCooldown;

    [SetsRequiredMembers]
    public Hulker(Vector2 position, IHasPosition target) : base(position, 1)
    {
        _followEntity = new FollowEntity(this, target, 0.04f);
        Collider = new RectangleCollider(this, 128f, 128f);
        Health = 120f;
    }

    public override float Experience => 6f;

    public ISpriteSheet SpriteSheet { get; } = new HulkerSpriteSheet();
    public IFrame CurrentFrame { get; private set; } = new HulkerSpriteSheet.LookDirectionFrame(Vector2.Zero);
    public Color? OutlineColor => null;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Velocity = _followEntity.CalculateVelocity(NearbyEnemies);

        if (_animationCooldown <= TimeSpan.Zero)
        {
            CurrentFrame = new HulkerSpriteSheet.LookDirectionFrame(Velocity);
            _animationCooldown = TimeSpan.FromMilliseconds(200);
        }

        _animationCooldown -= gameTime.ElapsedGameTime;
    }
}