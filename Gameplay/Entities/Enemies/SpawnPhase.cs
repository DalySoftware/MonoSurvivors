using System;
using System.Collections.Generic;
using System.Linq;
using CreateEnemy = System.Func<Microsoft.Xna.Framework.Vector2, Gameplay.Entities.Enemies.EnemyBase>;
using NumberToSpawn = int;

namespace Gameplay.Entities.Enemies;

public class SpawnPhase
{
    public required TimeSpan StartTime { get; init; }
    public required TimeSpan WaveCooldown { get; init; }

    /// <summary>
    ///     What to spawn each wave
    /// </summary>
    public required Dictionary<CreateEnemy, NumberToSpawn> EnemyWave { get; init; }

    public IEnumerable<CreateEnemy> GetEnemies()
    {
        foreach (var (enemyFunc, count) in EnemyWave)
        foreach (var func in Enumerable.Repeat(enemyFunc, count))
            yield return func;
    }
}