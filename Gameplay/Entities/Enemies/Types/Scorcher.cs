using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.SpriteSheets;

namespace Gameplay.Entities.Enemies.Types;

public class Scorcher : EnemyBase, ISpriteSheetVisual
{
    private readonly ScorcherSpriteSheet _spriteSheet;

    public Scorcher(SpawnContext spawnContext, bool elite) :
        base(spawnContext, ScorcherStats(elite), new FollowEntity(spawnContext.Player, 0.12f))
    {
        Colliders = [new RectangleCollider(this, 96f, 96f)];
        OutlineColor = elite ? ColorPalette.Cyan : null;
        _spriteSheet = new ScorcherSpriteSheet(spawnContext.Content);
        Behaviours =
        [
            new UpdateFrame(_spriteSheet),
        ];
    }

    public ISpriteSheet SpriteSheet => _spriteSheet;
    public IFrame CurrentFrame { get; } = new ScorcherSpriteSheet.DummyFrame();
    public Color? OutlineColor { get; }

    private static EnemyStats ScorcherStats(bool elite) => elite
        ? new EnemyStats(18f, 4f, 2, 2f)
        : new EnemyStats(12f, 2f, 1, 2f);

    private class UpdateFrame(ScorcherSpriteSheet spriteSheet) : IEnemyBehaviour
    {
        public void BeforeMove(GameTime gameTime) { }
        public void AfterMove(GameTime gameTime) => spriteSheet.Update(gameTime);
    }
}