using System;
using Gameplay.Audio;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Entities;
using Gameplay.Entities.Effects;

namespace Gameplay.Combat.Weapons.AreaOfEffect;

public class IceAura(
    PlayerCharacter owner,
    IEntityFinder entityFinder,
    IceAuraEffect auraEffect,
    IAudioPlayer audio,
    CritCalculator critCalculator)
    : IWeapon, IHasColliders
{
    private const float BaseDamage = 10f;
    private const float BaseRange = 150f;
    private readonly static SlowOnHit SlowOnHit =
        new(new MovementSlowdown(0.3f, TimeSpan.FromSeconds(1)));
    private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(1);

    private readonly CircleCollider _collider = new(owner, BaseRange);

    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    private WeaponBeltStats Stats => owner.WeaponBelt.Stats;

    private float Range => BaseRange * RangeMultiplier;
    private float RangeMultiplier { get; set; }

    public ICollider[] Colliders => [_collider];

    public void Update(GameTime gameTime)
    {
        if (Math.Abs(RangeMultiplier - Stats.RangeMultiplier) > 0.001f) OnRangeChange();

        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        DealDamage(gameTime);

        _remainingCooldown = _cooldown / Stats.AttackSpeedMultiplier;
    }

    private void OnRangeChange()
    {
        RangeMultiplier = Stats.RangeMultiplier;
        auraEffect.Range = Range;
        _collider.CollisionRadius = Range;
    }


    private void DealDamage(GameTime gameTime)
    {
        var damage = critCalculator.CalculateCritDamage(BaseDamage, Stats) * Stats.DamageMultiplier;
        var nearby = entityFinder.EnemiesCloseTo(owner.Position, Range * 1.5f);

        foreach (var enemy in nearby)
        {
            var colliders = enemy.Colliders;
            var overlapped = false;

            for (var c = 0; c < colliders.Length; c++)
                if (CollisionChecker.HasOverlap(colliders[c], _collider))
                {
                    overlapped = true;
                    break;
                }

            if (!overlapped)
                continue;

            enemy.TakeDamage(owner, damage);

            var context = new HitContext(gameTime, owner, enemy);

            SlowOnHit.Apply(context);
            var effects = owner.WeaponBelt.OnHitEffects;
            for (var i = 0; i < effects.Count; i++)
                effects[i].Apply(context);
        }

        auraEffect.SpawnRipple();
        audio.Play(SoundEffectTypes.IceAura);
    }
}