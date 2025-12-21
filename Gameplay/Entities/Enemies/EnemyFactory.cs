namespace Gameplay.Entities.Enemies;

public class EnemyFactory(PlayerCharacter player)
{
    public BasicEnemy BasicEnemy(Vector2 position) => new(position, player);

    public Hulker Hulker(Vector2 position) => new(position, player);

    public Scorcher Scorcher(Vector2 position) => new(position, player);
}