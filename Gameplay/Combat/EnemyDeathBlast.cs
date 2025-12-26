using System;
using Gameplay.Entities;
using Gameplay.Entities.Pooling;
using Gameplay.Utilities;

namespace Gameplay.Combat;

public class EnemyDeathBlast(BulletPool pool, EntityManager entityManager)
{
    private const float BaseDamage = 4f;

    public void Explode(PlayerCharacter owner, Vector2 position, int bullets, float damageMultiplier)
    {
        var bulletDirections = ArcSpreader.EvenlySpace(ArcSpreader.RandomDirection(), bullets, MathF.Tau);
        var damage = BaseDamage * damageMultiplier;
        var range = 100f * owner.WeaponBelt.Stats.RangeMultiplier;

        foreach (var direction in bulletDirections)
        {
            var bullet = pool.Get(owner, position, position + direction, 1f, damage, range);
            entityManager.Spawn(bullet);
        }
    }
}