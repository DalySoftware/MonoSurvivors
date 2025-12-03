using System;
using Entities;
using Entities.Enemy;
using Microsoft.Xna.Framework;

namespace GameLoop.Scenes.Gameplay;

internal class EnemySpawner(EntityManager entityManager, PlayerCharacter target) : IEntity
{
    private readonly Random _random = new();
    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    internal TimeSpan SpawnDelay { get; set; } = TimeSpan.FromSeconds(1);
    internal int BatchSize { get; set; } = 1;

    public void Update(GameTime gameTime)
    {
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        _remainingCooldown = SpawnDelay;
        for (var i = 0; i < BatchSize; i++)
            entityManager.Add(GetEnemyWithRandomPosition);
    }

    private BasicEnemy GetEnemyWithRandomPosition()
    {
        const float distanceFromPlayer = 500f;
        var angle = _random.NextDouble() * 2 * Math.PI;

        var x = target.Position.X + (float)Math.Cos(angle) * distanceFromPlayer;
        var y = target.Position.Y + (float)Math.Sin(angle) * distanceFromPlayer;

        var position = new Vector2(x, y);

        return new BasicEnemy(position, target);
    }
}