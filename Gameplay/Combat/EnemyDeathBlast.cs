using System;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Utilities;

namespace Gameplay.Combat;

public static class EnemyDeathBlast
{
    private const float BaseDamage = 4f;

    public static void Explode(EntityManager entityManager, PlayerCharacter owner, Vector2 position, int bullets,
        float damageMultiplier)
    {
        var bulletDirections = ArcSpreader.EvenlySpace(ArcSpreader.RandomDirection(), bullets, MathF.Tau);
        var damage = BaseDamage * damageMultiplier;

        foreach (var direction in bulletDirections)
        {
            var bullet = new Bullet(owner, position, position + direction, damage, 100f, speed: 1f);
            entityManager.Spawn(bullet);
        }
    }
}