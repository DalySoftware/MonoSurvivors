using System.Collections.Generic;
using System.Linq;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Entities;
using Gameplay.Levelling.PowerUps.Weapon;

namespace Gameplay.Combat.Weapons;

public class WeaponBelt(BulletSplitOnHit bulletSplit, ChainLightningOnHit chainLightning) : IEntity
{
    private readonly List<IWeaponPowerUp> _powerUps = [];
    private readonly List<IWeapon> _weapons = [];

    public WeaponBeltStats Stats { get; } = new();

    public List<IOnHitEffect> OnHitEffects { get; } =
    [
        bulletSplit,
        chainLightning,
    ];

    public void Update(GameTime gameTime) => _weapons.ForEach(w => w.Update(gameTime, Stats));

    public void AddWeapon(IWeapon weapon) => _weapons.Add(weapon);

    public void AddPowerUp(IWeaponPowerUp powerUp)
    {
        _powerUps.Add(powerUp);

        RefreshWeaponStats();
    }

    private void RefreshWeaponStats()
    {
        Stats.AttackSpeedMultiplier = _powerUps.OfType<AttackSpeedUp>().Sum(p => p.Value) + 1f;
        Stats.DamageMultiplier = _powerUps.OfType<DamageUp>().Sum(p => p.Value) + 1f;
        Stats.RangeMultiplier = _powerUps.OfType<RangeUp>().Sum(p => p.Value) + 1f;
        Stats.ExtraShots = _powerUps.OfType<ShotCountUp>().Sum(p => p.ExtraShots);
        Stats.Pierce = _powerUps.OfType<PierceUp>().Sum(p => p.Value);
        Stats.ProjectileSpeedMultiplier = _powerUps.OfType<ProjectileSpeedUp>().Sum(p => p.Value) + 1f;
        Stats.CritChance = _powerUps.OfType<CritChanceUp>().Sum(p => p.Value);
        Stats.CritDamage =
            _powerUps.OfType<CritDamageUp>().Sum(p => p.Value) + CritCalculator.BaseCritDamageMultiplier;
        Stats.ChainLightningChance = _powerUps.OfType<ChainLightningUp>().Sum(p => p.Value);

        var bulletSplit = _powerUps.OfType<BulletSplitUp>().Sum(p => p.Bullets);
        Stats.BulletSplit = bulletSplit <= 0 ? 0 : bulletSplit + 1; // never split into only 1 bullet
    }
}

public class WeaponBeltStats
{
    public float AttackSpeedMultiplier { get; set; } = 1f;
    public float ProjectileSpeedMultiplier { get; set; } = 1f;
    public float DamageMultiplier { get; set; } = 1f;
    public float RangeMultiplier { get; set; } = 1f;
    public int ExtraShots { get; set; } = 0;
    public int Pierce { get; set; } = 0;
    public float SpeedMultiplier { get; set; } = 1f;
    public float CritChance { get; set; } = 0f;
    public float CritDamage { get; set; } = CritCalculator.BaseCritDamageMultiplier;
    public int BulletSplit { get; set; } = 0;
    public float ChainLightningChance { get; set; } = 0f;
}