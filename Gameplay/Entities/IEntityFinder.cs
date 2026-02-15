using System.Collections.Generic;
using Gameplay.Behaviour;
using Gameplay.Entities.Enemies;

namespace Gameplay.Entities;

public interface IEntityFinder
{
    IReadOnlyList<EnemyBase> Enemies { get; }
    EnemyBase? NearestEnemyTo(IHasPosition source);
    IEnumerable<EnemyBase> EnemiesCloseTo(Vector2 position, float maxDistance);
}