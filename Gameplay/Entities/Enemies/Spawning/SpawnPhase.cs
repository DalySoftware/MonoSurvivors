using System;
using System.Collections.Generic;
using CreateEnemy = System.Func<Microsoft.Xna.Framework.Vector2, Gameplay.Entities.Enemies.EnemyBase>;

namespace Gameplay.Entities.Enemies.Spawning;

public sealed class SpawnPhase
{
    public required TimeSpan Duration { get; init; }
    public required float BudgetMultiplier { get; init; } = 1f;
    public required IReadOnlyList<SpawnEntry> Enemies { get; init; }

    public Func<Vector2, Action<EnemyBase>, EnemyBase>? BossFactory { get; init; } = null;

    internal bool BossSpawned { get; set; }
}

public sealed record SpawnEntry(CreateEnemy Factory, float Cost, float Weight = 1f);