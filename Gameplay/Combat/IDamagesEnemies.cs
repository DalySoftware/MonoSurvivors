using Gameplay.CollisionDetection;
using Gameplay.Entities.Enemies;

namespace Gameplay.Combat;

public interface IDamagesEnemies : IHasColliders
{
    public void OnHit(GameTime gameTime, EnemyBase enemy);
}