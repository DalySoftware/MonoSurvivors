using System;
using System.Collections.Generic;
using Gameplay.Levelling;

namespace Gameplay.Entities.Enemies;

public class EnemySpawner(EntityManager entityManager, PlayerCharacter target) : IEntity
{
    private readonly Random _random = new();
    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    public TimeSpan SpawnDelay { get; set; } = TimeSpan.FromSeconds(1);
    public int BatchSize { get; set; } = 1;

    public bool MarkedForDeletion => false;

    public void Update(GameTime gameTime)
    {
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        _remainingCooldown = SpawnDelay;
        for (var i = 0; i < BatchSize; i++)
            entityManager.Spawn(GetEnemyWithRandomPosition());
    }

    private BasicEnemy GetEnemyWithRandomPosition()
    {
        const float distanceFromPlayer = 500f;
        var angle = _random.NextDouble() * 2 * Math.PI;

        var x = target.Position.X + (float)Math.Cos(angle) * distanceFromPlayer;
        var y = target.Position.Y + (float)Math.Sin(angle) * distanceFromPlayer;

        var position = new Vector2(x, y);

        return new BasicEnemy(position, target)
        {
            OnDeath = OnDeath
        };
    }

    private void OnDeath(EnemyBase deadEnemy)
    {
        foreach (var experience in GetExperiences(deadEnemy))
            entityManager.Spawn(experience);
    }

    private IEnumerable<Experience> GetExperiences(EnemyBase deadEnemy)
    {
        for (var i = 0; i < deadEnemy.Experience; i++)
        {
            var position = deadEnemy.Position + new Vector2(_random.Next(-10, 10), _random.Next(-10, 10));
            yield return new Experience(position, 1f, target);
        }
    }
}