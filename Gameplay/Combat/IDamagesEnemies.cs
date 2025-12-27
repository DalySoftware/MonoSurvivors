using Gameplay.CollisionDetection;
using Gameplay.Entities.Enemies;

namespace Gameplay.Combat;

internal interface IDamagesEnemies : IHasCollider
{
    public float Damage { get; }
    public void OnHit(GameTime gameTime, EnemyBase enemy);
}