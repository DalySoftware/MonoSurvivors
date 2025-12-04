using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

internal interface IDamagesEnemies : ICircleCollider
{
    public float Damage { get; }
    public void OnHit() { }
}