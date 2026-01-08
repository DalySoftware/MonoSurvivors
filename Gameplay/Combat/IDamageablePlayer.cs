using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

internal interface IDamageablePlayer : IHasColliders
{
    public bool Damageable { get; }

    public void TakeDamage(int damage);
}