using System;
using System.Diagnostics.CodeAnalysis;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework.Content;

namespace Gameplay.Entities.Enemies.Types;

public class BasicEnemy : EnemyBase, ISpriteSheetVisual
{
    private readonly FollowEntity _followEntity;

    private TimeSpan _animationCooldown = TimeSpan.Zero;

    private BasicEnemySpriteSheet.LookDirectionFrame _frame = new(Vector2.Zero);

    [SetsRequiredMembers]
    public BasicEnemy(ContentManager content, Vector2 initialPosition, IHasPosition target, bool elite)
        : base(initialPosition, BasicEnemyStats(elite))
    {
        _followEntity = new FollowEntity(this, target, 0.07f);
        Colliders = [new CircleCollider(this, 32f)];
        OutlineColor = elite ? ColorPalette.Cyan : null;
        SpriteSheet = new BasicEnemySpriteSheet(content);
    }

    public ISpriteSheet SpriteSheet { get; }
    public Color? OutlineColor { get; }
    public IFrame CurrentFrame => _frame;

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
            _frame.Direction = Velocity;
            _animationCooldown = TimeSpan.FromMilliseconds(200);
        }

        _animationCooldown -= gameTime.ElapsedGameTime;
    }
}