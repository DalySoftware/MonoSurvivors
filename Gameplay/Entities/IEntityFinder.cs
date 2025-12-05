using Gameplay.Behaviour;
using Gameplay.Entities.Enemies;

namespace Gameplay.Entities;

public interface IEntityFinder
{
    public EnemyBase? NearestEnemyTo(IHasPosition source);
}