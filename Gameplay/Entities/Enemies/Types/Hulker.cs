using System;
using System.Diagnostics.CodeAnalysis;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;

namespace Gameplay.Entities.Enemies.Types;

public class Hulker : EnemyBase, ISpriteSheetVisual
{
    private readonly FollowEntity _followEntity;
    private TimeSpan _animationCooldown;

    [SetsRequiredMembers]
    public Hulker(Vector2 position, IHasPosition target, bool elite) : base(position, HulkerStats(elite))
    {
        _followEntity = new FollowEntity(this, target, 0.04f);
        Collider = new RectangleCollider(this, 128f, 128f);
        OutlineColor = elite ? ColorPalette.Cyan : null;
    }

    public ISpriteSheet SpriteSheet { get; } = new HulkerSpriteSheet();
    public IFrame CurrentFrame { get; private set; } = new HulkerSpriteSheet.LookDirectionFrame(Vector2.Zero);
    public Color? OutlineColor { get; }

    private static EnemyStats HulkerStats(bool elite) => elite
        ? new EnemyStats(500f, 20f, 1, 0.3f)
        : new EnemyStats(120f, 6f, 1, 0.3f);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        IntentVelocity = _followEntity.CalculateVelocity(NearbyEnemies);

        if (_animationCooldown <= TimeSpan.Zero)
        {
            CurrentFrame = new HulkerSpriteSheet.LookDirectionFrame(Velocity);
            _animationCooldown = TimeSpan.FromMilliseconds(200);
        }

        _animationCooldown -= gameTime.ElapsedGameTime;
    }
}