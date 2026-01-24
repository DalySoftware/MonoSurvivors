using System.Collections.Generic;
using Gameplay.Behaviour;
using Gameplay.Entities.Enemies;

namespace Gameplay.Entities;

public interface IEntityFinder
{
    EnemyBase? NearestEnemyTo(IHasPosition source);
    IEnumerable<EnemyBase> EnemiesCloseTo(Vector2 position, float maxDistance);
}