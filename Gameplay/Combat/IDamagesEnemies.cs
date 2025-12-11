using Gameplay.CollisionDetection;
using Gameplay.Entities.Enemies;

namespace Gameplay.Combat;

internal interface IDamagesEnemies : ICircleCollider
{
    public float Damage { get; }
    public void OnHit(EnemyBase enemy);
}