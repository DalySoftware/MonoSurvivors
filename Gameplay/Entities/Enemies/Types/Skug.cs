using System;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.SpriteSheets;

namespace Gameplay.Entities.Enemies.Types;

public class Skug : EnemyBase, ISpriteSheetVisual
{
    private readonly SkugSpriteSheet.LookDirectionFrame _frame = new(Vector2.Zero);

    public Skug(SpawnContext spawnContext, bool elite)
        : base(spawnContext, SkugStats(elite), new FollowEntity(spawnContext.Player, 0.07f), ColorPalette.Violet)
    {
        Colliders = [new CircleCollider(this, 32f)];
        OutlineColor = elite ? ColorPalette.Cyan : null;
        SpriteSheet = new SkugSpriteSheet(spawnContext.Content);
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

    private static EnemyStats SkugStats(bool elite) =>
        elite
            ? new EnemyStats(40f, 2f, 1)
            : new EnemyStats(20f, 1f, 1);
}