using System;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.SpriteSheets;
using Microsoft.Xna.Framework.Content;

namespace Gameplay.Entities.Enemies.Types;

public class Hulker : EnemyBase, ISpriteSheetVisual
{
    private readonly FollowEntity _followEntity;
    private TimeSpan _animationCooldown;

    public Hulker(ContentManager content, Vector2 position, IHasPosition target, bool elite,
        EnemyDeathHandler deathHandler) : base(position,
        HulkerStats(elite), deathHandler)
    {
        _followEntity = new FollowEntity(this, target, 0.04f);
        Colliders = [new RectangleCollider(this, 128f, 128f)];
        OutlineColor = elite ? ColorPalette.Cyan : null;
        SpriteSheet = new HulkerSpriteSheet(content);
    }

    public ISpriteSheet SpriteSheet { get; }
    public IFrame CurrentFrame { get; private set; } = new HulkerSpriteSheet.LookDirectionFrame(Vector2.Zero);
    public Color? OutlineColor { get; }

    private static EnemyStats HulkerStats(bool elite) => elite
        ? new EnemyStats(500f, 20f, 1, 0.3f)
        : new EnemyStats(120f, 6f, 1, 0.3f);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        IntentVelocity = _followEntity.CalculateVelocity(SeparationForce);

        if (_animationCooldown <= TimeSpan.Zero)
        {
            CurrentFrame = new HulkerSpriteSheet.LookDirectionFrame(Velocity);
            _animationCooldown = TimeSpan.FromMilliseconds(200);
        }

        _animationCooldown -= gameTime.ElapsedGameTime;
    }
}