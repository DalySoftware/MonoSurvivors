using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.SpriteSheets;
using Microsoft.Xna.Framework.Content;

namespace Gameplay.Entities.Enemies.Types;

public class Scorcher : EnemyBase, ISpriteSheetVisual
{
    private readonly FollowEntity _followEntity;
    private readonly ScorcherSpriteSheet _spriteSheet;

    public Scorcher(ContentManager content, Vector2 position, IHasPosition target, bool elite,
        EnemyDeathHandler deathHandler) : base(position,
        ScorcherStats(elite), deathHandler)
    {
        _followEntity = new FollowEntity(this, target, 0.12f);
        Colliders = [new RectangleCollider(this, 96f, 96f)];
        OutlineColor = elite ? ColorPalette.Cyan : null;
        _spriteSheet = new ScorcherSpriteSheet(content);
    }

    public ISpriteSheet SpriteSheet => _spriteSheet;
    public IFrame CurrentFrame { get; } = new ScorcherSpriteSheet.DummyFrame();
    public Color? OutlineColor { get; }

    private static EnemyStats ScorcherStats(bool elite) => elite
        ? new EnemyStats(18f, 4f, 2, 2f)
        : new EnemyStats(12f, 2f, 1, 2f);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        IntentVelocity = _followEntity.CalculateVelocity(SeparationForce);
        _spriteSheet.Update(gameTime);
    }
}