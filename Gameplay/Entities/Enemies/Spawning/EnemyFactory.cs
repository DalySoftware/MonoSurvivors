using System;
using Gameplay.Entities.Enemies.Types;
using Microsoft.Xna.Framework.Content;

namespace Gameplay.Entities.Enemies.Spawning;

public class EnemyFactory(PlayerCharacter player, ContentManager content, EnemyDeathHandler deathHandler)
{
    public BasicEnemy BasicEnemy(Vector2 position) => new(content, position, player, false, deathHandler);
    public BasicEnemy EliteBasicEnemy(Vector2 position) => new(content, position, player, true, deathHandler);

    public Hulker Hulker(Vector2 position) => new(content, position, player, false, deathHandler);
    public Hulker EliteHulker(Vector2 position) => new(content, position, player, true, deathHandler);

    public Scorcher Scorcher(Vector2 position) => new(content, position, player, false, deathHandler);
    public Scorcher EliteScorcher(Vector2 position) => new(content, position, player, true, deathHandler);

    public SnakeBoss SnakeBoss(Vector2 position, Action<EnemyBase> onDeath) =>
        new(content, position, player, onDeath, deathHandler);
}