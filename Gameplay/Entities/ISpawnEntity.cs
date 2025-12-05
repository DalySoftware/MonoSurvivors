using System.Collections.Generic;

namespace Gameplay.Entities;

public interface ISpawnEntity
{
    public void Spawn(params IEnumerable<IEntity> entity);
}