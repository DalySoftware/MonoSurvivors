using System;
using Gameplay.Entities.Enemies.Types;
using Microsoft.Xna.Framework.Content;

namespace Gameplay.Entities.Enemies.Spawning;

public class EnemyFactory(PlayerCharacter player, ContentManager content, EnemyDeathHandler deathHandler)
{
    private EnemyBase.SpawnContext SpawnContext(Vector2 position) => new(position, deathHandler, player, content);

    public BasicEnemy BasicEnemy(Vector2 position) => new(SpawnContext(position), false);
    public BasicEnemy EliteBasicEnemy(Vector2 position) => new(SpawnContext(position), true);

    public Hulker Hulker(Vector2 position) => new(SpawnContext(position), false);
    public Hulker EliteHulker(Vector2 position) => new(SpawnContext(position), true);

    public Scorcher Scorcher(Vector2 position) => new(SpawnContext(position), false);
    public Scorcher EliteScorcher(Vector2 position) => new(SpawnContext(position), true);

    public SnakeBoss SnakeBoss(Vector2 position, Action<EnemyBase> onDeath) => new(SpawnContext(position), onDeath);
}