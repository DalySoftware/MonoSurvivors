using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Utilities;

namespace Gameplay.Entities.Enemies.Spawning;

public sealed class EnemySpawner(
    EntityManager entityManager,
    PlayerCharacter player,
    EnemyFactory enemyFactory,
    ScreenPositioner screenPositioner)
{
    private const float BaseBudgetPerSecond = 0.5f;
    private readonly SpawnBudgeter _budgeter = new();

    private readonly List<SpawnPhase> _phases =
    [
        new()
        {
            Duration = TimeSpan.FromMinutes(2),
            BudgetMultiplier = 0.8f,
            Enemies =
            [
                new SpawnEntry(enemyFactory.BasicEnemy, 1f),
            ],
        },
        new()
        {
            Duration = TimeSpan.FromMinutes(2),
            BudgetMultiplier = 0.8f,
            Enemies =
            [
                new SpawnEntry(enemyFactory.BasicEnemy, 1f),
                new SpawnEntry(enemyFactory.EliteBasicEnemy, 3f, 0.05f),
            ],
        },
        new()
        {
            Duration = TimeSpan.FromMinutes(3),
            BudgetMultiplier = 1.0f,
            Enemies =
            [
                new SpawnEntry(enemyFactory.BasicEnemy, 1f),
                new SpawnEntry(enemyFactory.EliteBasicEnemy, 3f, 0.1f),
                new SpawnEntry(enemyFactory.Scorcher, 3f),
            ],
        },
        new()
        {
            Duration = TimeSpan.FromMinutes(4),
            BudgetMultiplier = 1.2f,
            Enemies =
            [
                new SpawnEntry(enemyFactory.BasicEnemy, 1f),
                new SpawnEntry(enemyFactory.EliteBasicEnemy, 3f, 0.2f),
                new SpawnEntry(enemyFactory.Scorcher, 3f),
                new SpawnEntry(enemyFactory.Hulker, 6f, 0.4f),
            ],
        },
        new()
        {
            Duration = TimeSpan.FromMinutes(4),
            BudgetMultiplier = 1.4f,
            Enemies =
            [
                new SpawnEntry(enemyFactory.BasicEnemy, 1f, 0.8f),
                new SpawnEntry(enemyFactory.EliteBasicEnemy, 3f, 0.4f),
                new SpawnEntry(enemyFactory.Scorcher, 3f),
                new SpawnEntry(enemyFactory.Hulker, 6f, 1.5f),
            ],
        },
    ];

    private TimeSpan _elapsed;

    public void Update(GameTime gameTime)
    {
        _elapsed += gameTime.ElapsedGameTime;

        var phase = CurrentPhase();
        var growth = GrowthFactor(_elapsed);

        var budgetPerSecond = BaseBudgetPerSecond * growth * phase.BudgetMultiplier;

        _budgeter.AddBudget(budgetPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds);

        var mostExpensiveEnemy = phase.Enemies.Max(e => e.Cost);
        var triggerBudget = 2f * mostExpensiveEnemy;

        if (!(_budgeter.Budget >= triggerBudget)) return;

        foreach (var entry in _budgeter.Spend(phase.Enemies))
        {
            var enemy = entry.Factory(screenPositioner.GetRandomOffScreenPosition(player.Position));
            entityManager.Spawn(enemy);
        }
    }

    private SpawnPhase CurrentPhase()
    {
        var t = _elapsed;

        foreach (var phase in _phases)
        {
            if (t < phase.Duration)
                return phase;

            t -= phase.Duration;
        }

        return _phases[^1];
    }

    private static float GrowthFactor(TimeSpan elapsed)
    {
        var minutes = (float)elapsed.TotalMinutes;

        const float linearFactor = 0.5f;
        const float linearEndInMinutes = 4f;

        if (minutes < linearEndInMinutes)
            return 1f + linearFactor * minutes;

        const float finalLinearFactor = 1f + linearFactor * linearEndInMinutes;
        const float exponentialBase = 1.3f;
        return finalLinearFactor * MathF.Pow(exponentialBase, minutes - linearEndInMinutes);
    }
}