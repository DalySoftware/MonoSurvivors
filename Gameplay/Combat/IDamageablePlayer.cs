using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

internal interface IDamageablePlayer : ICircleCollider
{
    public int Health { get; }
    public bool Damageable { get; }

    public void TakeDamage(int damage);
}