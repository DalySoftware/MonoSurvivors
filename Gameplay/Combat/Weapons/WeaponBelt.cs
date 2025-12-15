using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities;
using Gameplay.Levelling.PowerUps.Weapon;

namespace Gameplay.Combat.Weapons;

public class WeaponBelt : IEntity
{
    private readonly List<IWeaponPowerUp> _powerUps = [];
    private readonly WeaponBeltStats _stats = new();
    private readonly List<IWeapon> _weapons = [];

    public void Update(GameTime gameTime) => _weapons.ForEach(w => w.Update(gameTime, _stats));

    public void AddWeapon(IWeapon weapon) => _weapons.Add(weapon);

    public void AddPowerUp(IWeaponPowerUp powerUp)
    {
        _powerUps.Add(powerUp);

        RefreshWeaponStats();
    }

    private void RefreshWeaponStats()
    {
        _stats.AttackSpeedMultiplier = _powerUps.OfType<AttackSpeedUp>().Sum(p => p.Value) + 1f;
        _stats.DamageMultiplier = _powerUps.OfType<DamageUp>().Sum(p => p.Value) + 1f;
        _stats.RangeMultiplier = _powerUps.OfType<RangeUp>().Sum(p => p.Value) + 1f;
        _stats.ExtraShots = _powerUps.OfType<ShotCountUp>().Sum(p => p.ExtraShots);
        _stats.Pierce = _powerUps.OfType<PierceUp>().Sum(p => p.Value);
        _stats.SpeedMultiplier = _powerUps.OfType<ProjectileSpeedUp>().Sum(p => p.Value) + 1f;
        _stats.CritChance = _powerUps.OfType<CritChanceUp>().Sum(p => p.Value);
        _stats.CritDamage =
            _powerUps.OfType<CritDamageUp>().Sum(p => p.Value) + CritCalculator.BaseCritDamageMultiplier;

        var bulletSplit = _powerUps.OfType<BulletSplitUp>().Sum(p => p.Bullets);
        _stats.BulletSplit = bulletSplit <= 0 ? 0 : bulletSplit + 1; // never split into only 1 bullet
    }
}

public class WeaponBeltStats
{
    public float AttackSpeedMultiplier { get; set; } = 1f;
    public float DamageMultiplier { get; set; } = 1f;
    public float RangeMultiplier { get; set; } = 1f;
    public int ExtraShots { get; set; } = 0;
    public int Pierce { get; set; } = 0;
    public float SpeedMultiplier { get; set; } = 1f;
    public float CritChance { get; set; } = 0f;
    public float CritDamage { get; set; } = CritCalculator.BaseCritDamageMultiplier;
    public int BulletSplit { get; set; } = 0;
}