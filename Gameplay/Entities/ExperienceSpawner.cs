using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Audio;
using Gameplay.Entities.Effects;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;

namespace Gameplay.Entities;

public class ExperienceSpawner(EntityManager entityManager, IAudioPlayer audio)
{
    private readonly static int[] ExperienceDenominations = [5, 1];
    private readonly static TimeSpan SpawnDelay = DeathGlitch.Duration + TimeSpan.FromMilliseconds(10);
    private readonly DelayedSpawnQueue<SpawnRequest> _queue = new();

    public void Update(GameTime gameTime) => _queue.Update(gameTime, SpawnExperiences);

    internal void SpawnExperienceFor(EnemyBase deadEnemy) =>
        _queue.Enqueue(SpawnDelay, new SpawnRequest(deadEnemy.Position, deadEnemy.Experience));

    private void SpawnExperiences(SpawnRequest request)
    {
        var remaining = request.Amount;
        while (remaining > 0)
        {
            var value = ExperienceDenominations.First(d => d <= remaining);
            remaining -= value;

            var position = request.Position + new Vector2(Random.Shared.Next(-10, 10), Random.Shared.Next(-10, 10));
            entityManager.Spawn(new Experience(position, value, audio));
        }
    }

    private readonly record struct SpawnRequest(Vector2 Position, float Amount);

    private sealed class DelayedSpawnQueue<T>
    {
        private readonly PriorityQueue<T, double> _queue = new();
        private double _nowSeconds;

        public void Enqueue(TimeSpan delay, T req)
        {
            var due = _nowSeconds + delay.TotalSeconds;
            _queue.Enqueue(req, due);
        }

        public void Update(GameTime gameTime, Action<T> process)
        {
            _nowSeconds += gameTime.ElapsedGameTime.TotalSeconds;

            while (_queue.TryPeek(out _, out var due) && due <= _nowSeconds)
            {
                var req = _queue.Dequeue();
                process(req);
            }
        }
    }
}