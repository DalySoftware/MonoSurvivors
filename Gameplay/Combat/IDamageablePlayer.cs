using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

internal interface IDamageablePlayer : ICircleCollider
{
    public float Health { get; set; }
}