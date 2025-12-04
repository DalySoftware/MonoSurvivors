using Entities.CollisionDetection;

namespace Entities.Combat;

internal interface IDamagesEnemies : ICircleCollider
{
    public float Damage { get; }
    public void OnHit() { }
}