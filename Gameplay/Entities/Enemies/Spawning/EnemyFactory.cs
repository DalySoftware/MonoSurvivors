using Gameplay.Entities.Enemies.Types;
using Microsoft.Xna.Framework.Content;

namespace Gameplay.Entities.Enemies.Spawning;

public class EnemyFactory(PlayerCharacter player, ContentManager content)
{
    public BasicEnemy BasicEnemy(Vector2 position) => new(content, position, player, false);
    public BasicEnemy EliteBasicEnemy(Vector2 position) => new(content, position, player, true);

    public Hulker Hulker(Vector2 position) => new(content, position, player, false);
    public Hulker EliteHulker(Vector2 position) => new(content, position, player, true);

    public Scorcher Scorcher(Vector2 position) => new(content, position, player, false);
    public Scorcher EliteScorcher(Vector2 position) => new(content, position, player, true);

    public SnakeBoss SnakeBoss(Vector2 position) => new(content, position, player);
}