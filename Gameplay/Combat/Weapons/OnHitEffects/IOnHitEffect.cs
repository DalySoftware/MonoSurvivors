using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;
using Gameplay.Rendering.Colors;

namespace Gameplay.Combat.Weapons.OnHitEffects;

public interface IOnHitEffect
{
    int Priority { get; }
    void Apply(IHitContext context);
}

public interface IHitContext
{
    GameTime GameTime { get; }
    PlayerCharacter Owner { get; }
    EnemyBase Enemy { get; }
}

public sealed record HitContext(GameTime GameTime, PlayerCharacter Owner, EnemyBase Enemy) : IHitContext;

public sealed class BulletHitContext(GameTime gameTime, PlayerCharacter owner, EnemyBase enemy, Bullet bullet)
    : IHitContext
{
    public Bullet Bullet { get; } = bullet;

    internal bool ConsumePierce { get; private set; } = true;
    internal bool Bounce { get; private set; }
    internal bool Delete { get; private set; } = false;

    internal Vector2? BounceVelocity { get; private set; }
    internal float BounceDamageMultiplier { get; private set; } = 1f;
    public PlayerCharacter Owner { get; } = owner;
    public EnemyBase Enemy { get; } = enemy;
    public GameTime GameTime { get; } = gameTime;
    public Color EffectColor { get; } = Color.LerpOklch(bullet.EffectColor, enemy.EffectColor, 0.5f);

    public void RequestBounce(Vector2 newVelocity, float damageMultiplier = 1f)
    {
        Bounce = true;
        ConsumePierce = false;
        BounceVelocity = newVelocity;
        BounceDamageMultiplier = damageMultiplier;
    }
}