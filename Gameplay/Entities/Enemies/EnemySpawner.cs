using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Utilities;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Enemies;

public class EnemySpawner : IEntity
{
    private readonly EntityManager _entityManager;
    private readonly PlayerCharacter _player;
    private readonly ScreenPositioner _screenPositioner;
    private readonly List<SpawnPhase> _waves;
    private TimeSpan _cooldown;

    private TimeSpan _elapsedTime;

    public EnemySpawner(
        EntityManager entityManager,
        PlayerCharacter player,
        GraphicsDevice graphics)
    {
        _entityManager = entityManager;
        _player = player;

        var enemyFactory = new EnemyFactory(player);
        _screenPositioner = new ScreenPositioner(graphics, 0.3f);

        _waves =
        [
            new SpawnPhase
            {
                Duration = TimeSpan.FromMinutes(1),
                WaveCooldown = TimeSpan.FromSeconds(4),
                EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
                {
                    { enemyFactory.BasicEnemy, 4 },
                },
            },
            new SpawnPhase
            {
                Duration = TimeSpan.FromMinutes(1),
                WaveCooldown = TimeSpan.FromSeconds(3),
                EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
                {
                    { enemyFactory.BasicEnemy, 8 },
                },
            },
            new SpawnPhase
            {
                Duration = TimeSpan.FromMinutes(2),
                WaveCooldown = TimeSpan.FromSeconds(3),
                EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
                {
                    { enemyFactory.BasicEnemy, 10 },
                    { enemyFactory.Scorcher, 1 },
                },
            },
            new SpawnPhase
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
            new SpawnPhase
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
            new SpawnPhase
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
            new SpawnPhase
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
            new SpawnPhase
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
    }


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
            _entityManager.Spawn(enemyFactory(_screenPositioner.GetRandomOffScreenPosition(_player.Position)));
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