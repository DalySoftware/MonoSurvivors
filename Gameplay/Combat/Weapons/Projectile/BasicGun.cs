using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Levelling.PowerUps.Weapon;

namespace Gameplay.Combat.Weapons.Projectile;

public class BasicGun(PlayerCharacter owner, ISpawnEntity spawnEntity, IEntityFinder entityFinder, IAudioPlayer audio)
    : IWeapon
{
    private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(1);
    private TimeSpan _remainingCooldown = TimeSpan.Zero;

    public bool MarkedForDeletion => false;

    public void Update(GameTime gameTime, IReadOnlyCollection<IWeaponPowerUp> powerUps)
    {
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        _remainingCooldown = _cooldown;
        var damageMultiplier = powerUps.OfType<DamageUp>().Sum(p => p.Value) + 1f;
        Shoot(damageMultiplier);
    }

    private void Shoot(float damageMultiplier = 1f)
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        var damage = 10f * damageMultiplier;
        var bullet = new Bullet(owner.Position, target.Position, damage);
        spawnEntity.Spawn(bullet);
        audio.Play(SoundEffectTypes.Shoot);
    }
}