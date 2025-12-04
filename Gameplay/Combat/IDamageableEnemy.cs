using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

internal interface IDamageableEnemy : ICircleCollider
{
    public float Health { get; set; }
}