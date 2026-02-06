using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Utilities;

namespace Gameplay.Entities.Enemies.Spawning;

public sealed class EnemySpawner
{
    private const float BaseBudgetPerSecond = 0.5f;
    private readonly List<SpawnPhase> _phases;

    private readonly SpawnBudgeter _budgeter = new();
    private SpawnPhase? _previousPhase;
    private readonly List<EnemyBase> _activeBosses = [];
    private readonly EntityManager _entityManager;
    private readonly PlayerCharacter _player;
    private readonly ScreenPositioner _screenPositioner;
    private readonly IGlobalCommands _globalCommands;

    public EnemySpawner(EntityManager entityManager,
        PlayerCharacter player,
        EnemyFactory enemyFactory,
        ScreenPositioner screenPositioner,
        IGlobalCommands globalCommands)
    {
        _entityManager = entityManager;
        _player = player;
        _screenPositioner = screenPositioner;
        _globalCommands = globalCommands;
        _phases =
        [
            new SpawnPhase
            {
                Duration = TimeSpan.FromMinutes(2),
                BudgetMultiplier = 0.8f,
                Enemies =
                [
                    new SpawnEntry(enemyFactory.BasicEnemy, 1f),
                ],
            },
            new SpawnPhase
            {
                Duration = TimeSpan.FromMinutes(2),
                BudgetMultiplier = 0.8f,
                Enemies =
                [
                    new SpawnEntry(enemyFactory.BasicEnemy, 1f),
                    new SpawnEntry(enemyFactory.EliteBasicEnemy, 3f, 0.02f),
                ],
            },
            new SpawnPhase
            {
                Duration = TimeSpan.FromMinutes(3),
                BudgetMultiplier = 1.0f,
                Enemies =
                [
                    new SpawnEntry(enemyFactory.BasicEnemy, 1f),
                    new SpawnEntry(enemyFactory.EliteBasicEnemy, 3f, 0.05f),
                    new SpawnEntry(enemyFactory.Scorcher, 3f, 0.3f),
                ],
            },
            new SpawnPhase
            {
                Duration = TimeSpan.FromMinutes(4),
                BudgetMultiplier = 1.2f,
                Enemies =
                [
                    new SpawnEntry(enemyFactory.BasicEnemy, 1f),
                    new SpawnEntry(enemyFactory.EliteBasicEnemy, 3f, 0.1f),
                    new SpawnEntry(enemyFactory.Scorcher, 3f, 0.5f),
                    new SpawnEntry(enemyFactory.EliteScorcher, 9f, 0.05f),
                    new SpawnEntry(enemyFactory.Hulker, 6f, 0.4f),
                ],
            },
            new SpawnPhase // boss wave
            {
                Duration = TimeSpan.FromMinutes(2),
                BudgetMultiplier = 1.4f,
                Enemies =
                [
                    new SpawnEntry(enemyFactory.BasicEnemy, 1f, 0.8f),
                    new SpawnEntry(enemyFactory.EliteBasicEnemy, 3f, 0.2f),
                    new SpawnEntry(enemyFactory.Scorcher, 3f),
                    new SpawnEntry(enemyFactory.EliteScorcher, 9f, 0.1f),
                    new SpawnEntry(enemyFactory.Hulker, 6f, 1.5f),
                    new SpawnEntry(enemyFactory.EliteHulker, 20f, 0.05f),
                ],
                BossFactory = enemyFactory.SnakeBoss,
            },
        ];

        // Prefill the budget to avoid an awkward pause at game start
        var firstPhase = _phases[0];
        var mostExpensive = firstPhase.Enemies.Max(e => e.Cost);
        _budgeter.AddBudget(mostExpensive * 2f); // 2x to match trigger logic
    }

    public TimeSpan ElapsedTime { get; private set; }
    public IReadOnlyCollection<EnemyBase> ActiveBosses => _activeBosses;

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

        var mostExpensiveEnemy = 0f;
        foreach (var enemy in phase.Enemies)
            if (enemy.Cost > mostExpensiveEnemy)
                mostExpensiveEnemy = enemy.Cost;

        var triggerBudget = 2f * mostExpensiveEnemy;

        if (_budgeter.Budget < triggerBudget) return;

        foreach (var entry in _budgeter.Spend(phase.Enemies))
        {
            var enemy = entry.Factory(_screenPositioner.GetRandomOffScreenPosition(_player.Position));
            _entityManager.Spawn(enemy);
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

        var bossSpawnPosition = _screenPositioner.GetRandomOffScreenPosition(_player.Position);
        var boss = phase.BossFactory(bossSpawnPosition, OnBossKill);

        _entityManager.Spawn(boss);
        phase.BossSpawned = true;
        _activeBosses.Add(boss);
    }

    private void OnBossKill(EnemyBase deadBoss)
    {
        _activeBosses.Remove(deadBoss);
        _globalCommands.ShowWinGame();
    }

    private static float GrowthFactor(TimeSpan elapsed)
    {
        var minutes = (float)elapsed.TotalMinutes;

        const float linearFactor = 0.3f;
        const float linearEndInMinutes = 4f;

        if (minutes < linearEndInMinutes)
            return 1f + linearFactor * minutes;

        const float finalLinearFactor = 1f + linearFactor * linearEndInMinutes;
        const float exponentialBase = 1.33f;
        return finalLinearFactor * MathF.Pow(exponentialBase, minutes - linearEndInMinutes);
    }
}