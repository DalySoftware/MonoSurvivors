using System;
using System.Collections.Generic;
using Gameplay.Audio;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;

namespace Gameplay.Entities;

public class ExperienceSpawner(EntityManager entityManager, IAudioPlayer audio)
{
    internal void SpawnExperienceFor(EnemyBase deadEnemy, PlayerCharacter killer)
    {
        foreach (var experience in GetExperiences(deadEnemy))
            entityManager.Spawn(experience);
    }

    private IEnumerable<Experience> GetExperiences(EnemyBase deadEnemy)
    {
        for (var i = 0; i < deadEnemy.Experience; i++)
        {
            var position = deadEnemy.Position + new Vector2(Random.Shared.Next(-10, 10), Random.Shared.Next(-10, 10));
            yield return new Experience(position, 1f, audio);
        }
    }
}