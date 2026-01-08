using Gameplay.CollisionDetection;
using Gameplay.Entities.Enemies;

namespace Gameplay.Combat;

internal interface IDamagesEnemies : IHasColliders
{
    public void OnHit(GameTime gameTime, EnemyBase enemy);
}