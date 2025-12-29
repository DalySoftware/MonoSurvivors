using Gameplay.Entities.Enemies.Types;

namespace Gameplay.Entities.Enemies.Spawning;

public class EnemyFactory(PlayerCharacter player)
{
    public BasicEnemy BasicEnemy(Vector2 position) => new(position, player, false);
    public BasicEnemy EliteBasicEnemy(Vector2 position) => new(position, player, true);

    public Hulker Hulker(Vector2 position) => new(position, player, false);
    public Hulker EliteHulker(Vector2 position) => new(position, player, true);

    public Scorcher Scorcher(Vector2 position) => new(position, player, false);
    public Scorcher EliteScorcher(Vector2 position) => new(position, player, true);
}