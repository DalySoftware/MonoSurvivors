using System;
using Gameplay.Audio;
using Gameplay.Entities;

namespace Gameplay.Combat.Weapons.Projectile;

public class BasicGun(PlayerCharacter owner, ISpawnEntity spawnEntity, IEntityFinder entityFinder, IAudioPlayer audio)
    : IEntity
{
    private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(1);
    private TimeSpan _remainingCooldown = TimeSpan.Zero;

    public bool MarkedForDeletion => false;

    public void Update(GameTime gameTime)
    {
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        _remainingCooldown = _cooldown;
        Shoot();
    }

    private void Shoot()
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        var bullet = new Bullet(owner.Position, target.Position);
        spawnEntity.Spawn(bullet);
        audio.Play(SoundEffectTypes.Shoot);
    }
}