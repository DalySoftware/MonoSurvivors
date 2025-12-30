using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Utilities;

namespace Gameplay.Entities.Enemies.Spawning;

public sealed class EnemySpawner(
    EntityManager entityManager,
    PlayerCharacter player,
    EnemyFactory enemyFactory,
    ScreenPositioner screenPositioner,
    IGlobalCommands globalCommands)
{
    private const float BaseBudgetPerSecond = 0.5f;
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
                new SpawnEntry(enemyFactory.Scorcher, 3f, 0.5f),
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
                new SpawnEntry(enemyFactory.Scorcher, 3f, 0.5f),
                new SpawnEntry(enemyFactory.EliteScorcher, 9f, 0.05f),
                new SpawnEntry(enemyFactory.Hulker, 6f, 0.4f),
            ],
        },
        new() // boss wave
        {
            Duration = TimeSpan.FromMinutes(2),
            BudgetMultiplier = 1.4f,
            Enemies =
            [
                new SpawnEntry(enemyFactory.BasicEnemy, 1f, 0.8f),
                new SpawnEntry(enemyFactory.EliteBasicEnemy, 3f, 0.4f),
                new SpawnEntry(enemyFactory.Scorcher, 3f),
                new SpawnEntry(enemyFactory.EliteScorcher, 9f, 0.1f),
                new SpawnEntry(enemyFactory.Hulker, 6f, 1.5f),
                new SpawnEntry(enemyFactory.EliteHulker, 20f, 0.05f),
            ],
            BossFactory = enemyFactory.SnakeBoss,
        },
    ];

    private readonly SpawnBudgeter _budgeter = new();
    private SpawnPhase? _previousPhase;

    public TimeSpan ElapsedTime { get; private set; }

    public void Update(GameTime gameTime)
    {
        ElapsedTime += gameTime.ElapsedGameTime;

        var phase = CurrentPhase();
        if (phase != _previousPhase)
        {
            OnPhaseEntered(phase);
            _previousPhase = phase;
        }

        var growth = GrowthFactor(ElapsedTime);
        var budgetPerSecond = BaseBudgetPerSecond * growth * phase.BudgetMultiplier;

        _budgeter.AddBudget(budgetPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds);

        var mostExpensiveEnemy = phase.Enemies.Max(e => e.Cost);
        var triggerBudget = 2f * mostExpensiveEnemy;

        if (_budgeter.Budget < triggerBudget) return;

        foreach (var entry in _budgeter.Spend(phase.Enemies))
        {
            var enemy = entry.Factory(screenPositioner.GetRandomOffScreenPosition(player.Position));
            entityManager.Spawn(enemy);
        }
    }

    private SpawnPhase CurrentPhase()
    {
        var t = ElapsedTime;

        foreach (var phase in _phases)
        {
            if (t < phase.Duration)
                return phase;

            t -= phase.Duration;
        }

        return _phases[^1];
    }

    private void OnPhaseEntered(SpawnPhase phase)
    {
        if (phase.BossFactory is null || phase.BossSpawned)
            return;

        var bossSpawnPosition = screenPositioner.GetRandomOffScreenPosition(player.Position);
        var boss = phase.BossFactory(bossSpawnPosition, globalCommands.ShowWinGame);

        entityManager.Spawn(boss);
        phase.BossSpawned = true;
    }

    private static float GrowthFactor(TimeSpan elapsed)
    {
        var minutes = (float)elapsed.TotalMinutes;

        const float linearFactor = 0.3f;
        const float linearEndInMinutes = 4f;

        if (minutes < linearEndInMinutes)
            return 1f + linearFactor * minutes;

        const float finalLinearFactor = 1f + linearFactor * linearEndInMinutes;
        const float exponentialBase = 1.3f;
        return finalLinearFactor * MathF.Pow(exponentialBase, minutes - linearEndInMinutes);
    }
}