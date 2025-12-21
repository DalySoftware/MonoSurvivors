using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Audio;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;

namespace Gameplay.Entities;

public class ExperienceSpawner(EntityManager entityManager, IAudioPlayer audio)
{
    private readonly static int[] ExperienceDenominations = [5, 1];
    internal void SpawnExperienceFor(EnemyBase deadEnemy, PlayerCharacter killer)
    {
        foreach (var experience in GetExperiences(deadEnemy))
            entityManager.Spawn(experience);
    }

    private IEnumerable<Experience> GetExperiences(EnemyBase deadEnemy)
    {
        var remaining = deadEnemy.Experience;
        while (remaining > 0)
        {
            var value = ExperienceDenominations.First(d => d <= remaining);
            remaining -= value;

            var position = deadEnemy.Position + new Vector2(Random.Shared.Next(-10, 10), Random.Shared.Next(-10, 10));
            yield return new Experience(position, value, audio);
        }
    }
}