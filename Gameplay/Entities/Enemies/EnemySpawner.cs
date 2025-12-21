using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Utilities;

namespace Gameplay.Entities.Enemies;

public class EnemySpawner(
    EntityManager entityManager,
    PlayerCharacter player,
    EnemyFactory enemyFactory,
    ScreenPositioner screenPositioner)
    : IEntity
{
    private readonly List<SpawnPhase> _waves =
    [
        new()
        {
            Duration = TimeSpan.FromMinutes(1),
            WaveCooldown = TimeSpan.FromSeconds(4),
            EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
            {
                { enemyFactory.BasicEnemy, 4 },
            },
        },
        new()
        {
            Duration = TimeSpan.FromMinutes(1),
            WaveCooldown = TimeSpan.FromSeconds(3),
            EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
            {
                { enemyFactory.BasicEnemy, 8 },
            },
        },
        new()
        {
            Duration = TimeSpan.FromMinutes(2),
            WaveCooldown = TimeSpan.FromSeconds(3),
            EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
            {
                { enemyFactory.BasicEnemy, 10 },
                { enemyFactory.Scorcher, 1 },
            },
        },
        new()
        {
            Duration = TimeSpan.FromMinutes(2),
            WaveCooldown = TimeSpan.FromSeconds(3),
            EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
            {
                { enemyFactory.BasicEnemy, 10 },
                { enemyFactory.Scorcher, 2 },
                { enemyFactory.Hulker, 1 },
            },
        },
        new()
        {
            Duration = TimeSpan.FromMinutes(2),
            WaveCooldown = TimeSpan.FromSeconds(2),
            EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
            {
                { enemyFactory.BasicEnemy, 5 },
                { enemyFactory.Scorcher, 4 },
                { enemyFactory.Hulker, 4 },
            },
        },
        new()
        {
            Duration = TimeSpan.FromMinutes(2),
            WaveCooldown = TimeSpan.FromSeconds(1),
            EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
            {
                { enemyFactory.BasicEnemy, 5 },
                { enemyFactory.Scorcher, 8 },
                { enemyFactory.Hulker, 6 },
            },
        },
        new()
        {
            Duration = TimeSpan.FromMinutes(2),
            WaveCooldown = TimeSpan.FromSeconds(1),
            EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
            {
                { enemyFactory.BasicEnemy, 2 },
                { enemyFactory.Scorcher, 12 },
                { enemyFactory.Hulker, 10 },
            },
        },
        new()
        {
            Duration = TimeSpan.FromMinutes(2),
            WaveCooldown = TimeSpan.FromSeconds(1),
            EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
            {
                { enemyFactory.Scorcher, 12 },
                { enemyFactory.Hulker, 20 },
            },
        },
    ];

    private TimeSpan _cooldown;
    private TimeSpan _elapsedTime;

    public bool MarkedForDeletion => false;

    public void Update(GameTime gameTime)
    {
        _elapsedTime += gameTime.ElapsedGameTime;
        _cooldown -= gameTime.ElapsedGameTime;

        if (_cooldown > TimeSpan.Zero)
            return;

        var wave = CurrentPhase();
        _cooldown = wave.WaveCooldown;

        foreach (var enemyFactory in wave.GetEnemies().Shuffle())
            entityManager.Spawn(enemyFactory(screenPositioner.GetRandomOffScreenPosition(player.Position)));
    }

    private SpawnPhase CurrentPhase()
    {
        var timeToFill = _elapsedTime;
        foreach (var wave in _waves)
        {
            if (timeToFill < wave.Duration)
                return wave;

            timeToFill -= wave.Duration;
        }

        // If we run past all durations, stick on the last phase
        return _waves[^1];
    }
}