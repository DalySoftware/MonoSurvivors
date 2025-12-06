using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

internal interface IDamageablePlayer : ICircleCollider
{
    public int Health { get; set; }
}