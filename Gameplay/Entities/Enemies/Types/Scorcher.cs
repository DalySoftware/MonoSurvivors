using System.Diagnostics.CodeAnalysis;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies.Types;

public class Scorcher : EnemyBase, ISpriteSheetVisual
{
    private readonly FollowEntity _followEntity;
    private readonly ScorcherSpriteSheet _spriteSheet = new();

    [SetsRequiredMembers]
    public Scorcher(Vector2 position, IHasPosition target) : base(position, ScorcherStats())
    {
        _followEntity = new FollowEntity(this, target, 0.12f);
        Collider = new RectangleCollider(this, 96f, 96f);
    }

    public ISpriteSheet SpriteSheet => _spriteSheet;
    public IFrame CurrentFrame { get; } = new ScorcherSpriteSheet.DummyFrame();
    public Color? OutlineColor => null;

    private static EnemyStats ScorcherStats() => new(12f, 2f, 1, 2f);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        IntentVelocity = _followEntity.CalculateVelocity(NearbyEnemies);
        _spriteSheet.Update(gameTime);
    }
}