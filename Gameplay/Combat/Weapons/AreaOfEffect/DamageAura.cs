using System;
using System.Linq;
using Gameplay.CollisionDetection;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Entities;
using Gameplay.Entities.Effects;

namespace Gameplay.Combat.Weapons.AreaOfEffect;

public class DamageAura(PlayerCharacter owner, IEntityFinder entityFinder, DamageAuraEffect auraEffect)
    : IWeapon, IHasCollider
{
    private const float BaseDamage = 10f;
    private const float BaseRange = 150f;
    private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(1);

    private readonly CircleCollider _collider = new(owner, BaseRange);

    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    private WeaponBeltStats Stats => owner.WeaponBelt.Stats;

    private float Range => BaseRange * RangeMultiplier;
    private float RangeMultiplier { get; set; }
    public ICollider Collider => _collider;

    public void Update(GameTime gameTime)
    {
        if (Math.Abs(RangeMultiplier - Stats.RangeMultiplier) > 0.001f) OnRangeChange();

        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        DealDamage();

        _remainingCooldown = _cooldown / Stats.AttackSpeedMultiplier;
    }

    private void OnRangeChange()
    {
        RangeMultiplier = Stats.RangeMultiplier;
        auraEffect.Range = Range;
        _collider.CollisionRadius = Range;
    }

    private void DealDamage()
    {
        var damage = CritCalculator.CalculateCrit(BaseDamage, Stats) * Stats.DamageMultiplier;
        var nearby = entityFinder.EnemiesCloseTo(owner.Position, Range * 1.5f);

        foreach (var enemy in nearby.Where(e => CollisionChecker.HasOverlap(e.Collider, Collider)))
        {
            enemy.TakeDamage(owner, damage);
            foreach (var onHit in owner.WeaponBelt.OnHitEffects)
            {
                var context = new HitContext(owner, enemy);
                onHit.Apply(context);
            }
        }
    }
}