using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

public interface IDamageablePlayer : IHasColliders
{
    public bool Damageable { get; }

    public void TakeDamage(int damage);
}