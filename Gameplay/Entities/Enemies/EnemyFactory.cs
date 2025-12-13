using System;

namespace Gameplay.Entities.Enemies;

public class EnemyFactory(PlayerCharacter player, Action<EnemyBase> onDeath)
{
    public BasicEnemy BasicEnemy(Vector2 position) => new(position, player)
    {
        OnDeath = onDeath
    };

    public Hulker Hulker(Vector2 position) => new(position, player)
    {
        OnDeath = onDeath
    };
}