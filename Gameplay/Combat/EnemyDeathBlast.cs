using System;
using Gameplay.Audio;
using Gameplay.Combat.Weapons;
using Gameplay.Entities;
using Gameplay.Entities.Pooling;
using Gameplay.Rendering.Effects;
using Gameplay.Utilities;

namespace Gameplay.Combat;

public class EnemyDeathBlast(
    BulletPool pool,
    EntityManager entityManager,
    CritCalculator critCalculator,
    IAudioPlayer audio,
    CameraShake cameraShake)
{
    private const float BaseDamage = 4f;

    public void Explode(PlayerCharacter owner, Vector2 position, int bullets, float damageMultiplier)
    {
        var bulletDirections = ArcSpreader.EvenlySpace(ArcSpreader.RandomDirection(), bullets, MathF.Tau);
        var damage = critCalculator.CalculateCritDamage(BaseDamage * damageMultiplier, owner.WeaponBelt.Stats);
        var range = 100f * owner.WeaponBelt.Stats.RangeMultiplier;

        foreach (var direction in bulletDirections)
        {
            var bullet = pool.Get(BulletType.BasicSmall, owner, position, position + direction, 1f, damage, range, []);
            entityManager.Spawn(bullet);
        }

        audio.Play(SoundEffectTypes.EnemyExplode);
        var impulse = Vector2.Normalize(owner.Position - position) * (180f + 20f * bullets);
        cameraShake.AddImpulse(impulse);
    }
}