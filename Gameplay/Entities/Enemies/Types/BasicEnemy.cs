using System;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.SpriteSheets;
using Microsoft.Xna.Framework.Content;

namespace Gameplay.Entities.Enemies.Types;

public class BasicEnemy : EnemyBase, ISpriteSheetVisual
{
    private readonly BasicEnemySpriteSheet.LookDirectionFrame _frame = new(Vector2.Zero);

    public BasicEnemy(ContentManager content, Vector2 initialPosition, IHasPosition target, bool elite,
        EnemyDeathHandler deathHandler)
        : base(initialPosition, BasicEnemyStats(elite), deathHandler, new FollowEntity(target, 0.07f))
    {
        Colliders = [new CircleCollider(this, 32f)];
        OutlineColor = elite ? ColorPalette.Cyan : null;
        SpriteSheet = new BasicEnemySpriteSheet(content);
        Behaviours =
        [
            new ApplyVectorEvery(
                () => IntentVelocity,
                v => _frame.Direction = v,
                TimeSpan.FromMilliseconds(200)),
        ];
    }

    public ISpriteSheet SpriteSheet { get; }
    public Color? OutlineColor { get; }
    public IFrame CurrentFrame => _frame;

    private static EnemyStats BasicEnemyStats(bool elite) =>
        elite
            ? new EnemyStats(40f, 2f, 1)
            : new EnemyStats(20f, 1f, 1);
}