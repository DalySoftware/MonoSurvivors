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
    private readonly HulkerSpriteSheet.LookDirectionFrame _frame = new(Vector2.Zero);
    public Hulker(ContentManager content, Vector2 position, IHasPosition target, bool elite,
        EnemyDeathHandler deathHandler) : base(position,
        HulkerStats(elite), deathHandler, new FollowEntity(target, 0.04f))
    {
        Colliders = [new RectangleCollider(this, 128f, 128f)];
        OutlineColor = elite ? ColorPalette.Cyan : null;
        SpriteSheet = new HulkerSpriteSheet(content);
        Behaviours =
        [
            new ApplyVectorEvery(
                () => IntentVelocity,
                v => _frame.Direction = v,
                TimeSpan.FromMilliseconds(200)),
        ];
    }
    public IFrame CurrentFrame => _frame;
    public ISpriteSheet SpriteSheet { get; }
    public Color? OutlineColor { get; }

    private static EnemyStats HulkerStats(bool elite) => elite
        ? new EnemyStats(500f, 20f, 1, 0.3f)
        : new EnemyStats(120f, 6f, 1, 0.3f);
}