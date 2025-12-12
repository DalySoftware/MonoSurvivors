using System;
using System.Collections.Generic;
using Gameplay.Audio;
using Gameplay.Levelling;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Enemies;

public class EnemySpawner(
    EntityManager entityManager,
    PlayerCharacter player,
    IAudioPlayer audio,
    GraphicsDevice graphics) : IEntity
{
    private readonly Random _random = new();
    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    public required TimeSpan SpawnDelay { get; set; }
    public int BatchSize { get; set; } = 1;

    public bool MarkedForDeletion => false;

    public void Update(GameTime gameTime)
    {
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        _remainingCooldown = SpawnDelay;
        for (var i = 0; i < BatchSize; i++)
            entityManager.Spawn(GetEnemyWithRandomPosition(BasicEnemy));

        entityManager.Spawn(GetEnemyWithRandomPosition(Hulker));
    }

    private BasicEnemy BasicEnemy(Vector2 position) => new(position, player)
    {
        OnDeath = OnDeath
    };

    private Hulker Hulker(Vector2 position) => new(position, player)
    {
        OnDeath = OnDeath
    };

    private Vector2 GetRandomPosition()
    {
        // Random angle
        var angle = (float)_random.NextDouble() * 2f * MathF.PI;

        // Calculate distance based on angle to account for rectangular viewport
        var halfWidth = graphics.Viewport.Width * 0.5f;
        var halfHeight = graphics.Viewport.Height * 0.5f;
        var cos = MathF.Abs(MathF.Cos(angle));
        var sin = MathF.Abs(MathF.Sin(angle));

        // Distance to edge of rectangle at this angle, plus a small buffer
        // The buffer makes sure it's slightly offscreen, unless the player outruns camera by a crazy speed
        var distanceFromPlayer = MathF.Min(halfWidth / cos, halfHeight / sin) * 1.3f;

        var x = player.Position.X + MathF.Cos(angle) * distanceFromPlayer;
        var y = player.Position.Y + MathF.Sin(angle) * distanceFromPlayer;

        return new Vector2(x, y);
    }

    private EnemyBase GetEnemyWithRandomPosition(Func<Vector2, EnemyBase> factory) => factory(GetRandomPosition());

    private void OnDeath(EnemyBase deadEnemy)
    {
        foreach (var experience in GetExperiences(deadEnemy))
            entityManager.Spawn(experience);
        audio.Play(SoundEffectTypes.EnemyDeath);
        player.TrackKills(1);
    }

    private IEnumerable<Experience> GetExperiences(EnemyBase deadEnemy)
    {
        for (var i = 0; i < deadEnemy.Experience; i++)
        {
            var position = deadEnemy.Position + new Vector2(_random.Next(-10, 10), _random.Next(-10, 10));
            yield return new Experience(position, 1f, player, audio);
        }
    }
}