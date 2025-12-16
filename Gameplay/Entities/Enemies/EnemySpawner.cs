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
                StartTime = TimeSpan.Zero,
                WaveCooldown = TimeSpan.FromSeconds(5),
                EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
                {
                    { enemyFactory.BasicEnemy, 4 },
                },
            },
            new SpawnPhase
            {
                StartTime = TimeSpan.FromMinutes(2),
                WaveCooldown = TimeSpan.FromSeconds(5),
                EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
                {
                    { enemyFactory.BasicEnemy, 10 },
                    { enemyFactory.Hulker, 1 },
                },
            },
            new SpawnPhase
            {
                StartTime = TimeSpan.FromMinutes(4),
                WaveCooldown = TimeSpan.FromSeconds(3),
                EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
                {
                    { enemyFactory.BasicEnemy, 10 },
                    { enemyFactory.Hulker, 2 },
                },
            },
            new SpawnPhase
            {
                StartTime = TimeSpan.FromMinutes(6),
                WaveCooldown = TimeSpan.FromSeconds(2),
                EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
                {
                    { enemyFactory.BasicEnemy, 5 },
                    { enemyFactory.Hulker, 5 },
                },
            },
            new SpawnPhase
            {
                StartTime = TimeSpan.FromMinutes(8),
                WaveCooldown = TimeSpan.FromSeconds(2),
                EnemyWave = new Dictionary<Func<Vector2, EnemyBase>, int>
                {
                    { enemyFactory.BasicEnemy, 5 },
                    { enemyFactory.Hulker, 10 },
                },
            },
        ];
    }

    private SpawnPhase CurrentPhase => _waves.Last(w => _elapsedTime >= w.StartTime);

    public bool MarkedForDeletion => false;

    public void Update(GameTime gameTime)
    {
        _elapsedTime += gameTime.ElapsedGameTime;
        _cooldown -= gameTime.ElapsedGameTime;

        if (_cooldown > TimeSpan.Zero)
            return;

        var wave = CurrentPhase;
        _cooldown = wave.WaveCooldown;

        foreach (var enemyFactory in wave.GetEnemies().Shuffle())
            _entityManager.Spawn(
                enemyFactory(_screenPositioner.GetRandomOffScreenPosition(_player.Position))
            );
    }
}