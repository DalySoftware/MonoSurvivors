using System;
using System.Collections.Generic;
using ContentLibrary;
using Gameplay.Combat.Weapons.AreaOfEffect;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Levelling.PowerUps;

public static class PowerUpCatalog
{
    internal readonly static Dictionary<Type, PowerUpCategory> Categories = new()
    {
        // Damage
        [typeof(DamageUp)] = PowerUpCategory.Damage,
        [typeof(AttackSpeedUp)] = PowerUpCategory.Damage,

        // DamageEffects
        [typeof(ShotCountUp)] = PowerUpCategory.DamageEffects,
        [typeof(PierceUp)] = PowerUpCategory.DamageEffects,
        [typeof(BulletSplitUp)] = PowerUpCategory.DamageEffects,
        [typeof(ExplodeOnKillUp)] = PowerUpCategory.DamageEffects,
        [typeof(ChainLightningUp)] = PowerUpCategory.DamageEffects,

        // Health
        [typeof(MaxHealthUp)] = PowerUpCategory.Health,
        [typeof(HealthRegenUp)] = PowerUpCategory.Health,
        [typeof(LifeStealUp)] = PowerUpCategory.Health,

        // Speed
        [typeof(SpeedUp)] = PowerUpCategory.Speed,

        // Utility
        [typeof(PickupRadiusUp)] = PowerUpCategory.Utility,
        [typeof(ExperienceUp)] = PowerUpCategory.Utility,
        [typeof(RangeUp)] = PowerUpCategory.Utility,
        [typeof(ProjectileSpeedUp)] = PowerUpCategory.Utility,

        // Crit
        [typeof(CritChanceUp)] = PowerUpCategory.Crit,
        [typeof(CritDamageUp)] = PowerUpCategory.Crit,

        // WeaponUnlock
        [typeof(WeaponUnlock<Shotgun>)] = PowerUpCategory.WeaponUnlock,
        [typeof(WeaponUnlock<SniperRifle>)] = PowerUpCategory.WeaponUnlock,
        [typeof(WeaponUnlock<DamageAura>)] = PowerUpCategory.WeaponUnlock,
    };

    extension(IPowerUp powerUp)
    {
        public string Title() => powerUp switch
        {
            MaxHealthUp => "Increase Max Health",
            HealthRegenUp => "Increase Health Regen",
            SpeedUp => "Increase Speed",
            PickupRadiusUp => "Increase Pickup Radius",
            DamageUp => "Increase Damage",
            AttackSpeedUp => "Increase Attack Speed",
            ShotCountUp => "Increase Shot Count",
            RangeUp => "Increase Range",
            LifeStealUp => "Increase Life Steal",
            ExperienceUp => "Increase Experience Multiplier",
            CritChanceUp => "Increase Critical Hit Chance",
            CritDamageUp => "Increase Critical Hit Damage",
            PierceUp => "Pierce more enemies",
            ProjectileSpeedUp => "Increase Projectile Speed",
            BulletSplitUp => "Increase Bullet Split",
            ExplodeOnKillUp => "Increase on kill explosion",
            ChainLightningUp => "Increase Chain Lightning chance",
            WeaponUnlock<Shotgun> => "Unlock the Shotgun",
            WeaponUnlock<SniperRifle> => "Unlock the Sniper Rifle",
            WeaponUnlock<DamageAura> => "Unlock a damaging aura",
            _ => throw new ArgumentOutOfRangeException(nameof(powerUp)),
        };

        public string Description() => powerUp switch
        {
            MaxHealthUp maxHealthUp => $"Increase Max Health by {(maxHealthUp.Value / 2).HeartLabel()}",
            HealthRegenUp => "Increase Health Regen",
            SpeedUp speedUp => $"Increase Speed by {speedUp.Value:P0}",
            PickupRadiusUp pickupRadiusUp => $"Increase Pickup Radius by {pickupRadiusUp.Value:P0}",
            DamageUp damageUp => $"Increase Damage by {damageUp.Value:P0}",
            AttackSpeedUp attackSpeedUp => $"Increase Attack Speed by {attackSpeedUp.Value:P0}",
            ShotCountUp shotCountUp => $"Fire {shotCountUp.ExtraShots} extra shots",
            RangeUp rangeUp => $"Increase Range by {rangeUp.Value:P0}",
            LifeStealUp => "Increase Life Steal",
            ExperienceUp experienceUp => $"Increase Experience Multiplier by {experienceUp.Value:P0}",
            CritChanceUp critChanceUp => $"Increase Critical Hit Chance by {critChanceUp.Value:P0}",
            CritDamageUp critDamageUp => $"Increase Critical Hit Damage by {critDamageUp.Value:P0}",
            PierceUp pierceUp => $"Projectiles pierce {pierceUp.Value} more {pierceUp.Value.EnemiesLabel()}",
            ProjectileSpeedUp projectileSpeedUp => $"Increase Projectile Speed by {projectileSpeedUp.Value:P0}",
            BulletSplitUp => "Increase Bullet Split",
            ExplodeOnKillUp => "Increase explosion size on kill",
            ChainLightningUp => "Increase chance to trigger chain lightning on hit",
            WeaponUnlock<Shotgun> => "Unlock an extra weapon. The shotgun fires bullets in a spread",
            WeaponUnlock<SniperRifle> => "Unlock an extra weapon. The sniper rifle fires high damage shots",
            WeaponUnlock<DamageAura> => "Unlock an extra weapon. You emit an aura which damages all nearby enemies",
            _ => throw new ArgumentOutOfRangeException(nameof(powerUp)),
        };
    }

    private static class Colors
    {
        internal readonly static Color Health = new OklchColor(0.7f, 0.16f, 23).ToColor();
        internal readonly static Color Speed = new OklchColor(0.7f, 0.16f, 80).ToColor();
        internal readonly static Color Utility = new OklchColor(0.7f, 0.16f, 160).ToColor();
        internal readonly static Color Special = new OklchColor(0.7f, 0.16f, 215).ToColor();
        internal readonly static Color Damage = new OklchColor(0.7f, 0.16f, 295).ToColor();
        internal readonly static Color Crit = new OklchColor(0.7f, 0.16f, 340).ToColor();
        internal readonly static Color DamageEffects = new OklchColor(0.8f, 0.20f, 125).ToColor();
    }

    extension(IPowerUp? powerUp)
    {
        public Color BaseColor()
        {
            if (powerUp is null) return Color.Gold;

            return Categories[powerUp.GetType()] switch
            {
                PowerUpCategory.Damage => Colors.Damage,
                PowerUpCategory.DamageEffects => Colors.DamageEffects,
                PowerUpCategory.Health => Colors.Health,
                PowerUpCategory.Speed => Colors.Speed,
                PowerUpCategory.Utility => Colors.Utility,
                PowerUpCategory.Crit => Colors.Crit,
                PowerUpCategory.WeaponUnlock => Colors.Special,
                _ => throw new ArgumentOutOfRangeException(nameof(powerUp)),
            };
        }
    }
}

public enum PowerUpCategory
{
    Damage,
    DamageEffects,
    Health,
    Speed,
    Utility,
    Crit,
    WeaponUnlock,
}

public class PowerUpIcons(ContentManager content)
{
    private readonly Texture2D _attackSpeed = content.Load<Texture2D>(Paths.Images.PowerUpIcons.AttackSpeed);
    private readonly Texture2D _critChance = content.Load<Texture2D>(Paths.Images.PowerUpIcons.CritChance);
    private readonly Texture2D _critDamage = content.Load<Texture2D>(Paths.Images.PowerUpIcons.CritDamage);
    private readonly Texture2D _damage = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Damage);
    private readonly Texture2D _experience = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Experience);
    private readonly Texture2D _lifeSteal = content.Load<Texture2D>(Paths.Images.PowerUpIcons.LifeSteal);
    private readonly Texture2D _maxHealth = content.Load<Texture2D>(Paths.Images.PowerUpIcons.MaxHealth);
    private readonly Texture2D _pickupRadius = content.Load<Texture2D>(Paths.Images.PowerUpIcons.PickupRadius);
    private readonly Texture2D _pierce = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Pierce);
    private readonly Texture2D _projectileSpeed = content.Load<Texture2D>(Paths.Images.PowerUpIcons.ProjectileSpeed);
    private readonly Texture2D _range = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Range);
    private readonly Texture2D _shotCount = content.Load<Texture2D>(Paths.Images.PowerUpIcons.ShotCount);
    private readonly Texture2D _speed = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Speed);

    public Texture2D? IconFor(Node node) => node.PowerUp switch
    {
        ExperienceUp => _experience,
        LifeStealUp => _lifeSteal,
        MaxHealthUp => _maxHealth,
        PickupRadiusUp => _pickupRadius,
        SpeedUp => _speed,
        AttackSpeedUp => _attackSpeed,
        CritChanceUp => _critChance,
        CritDamageUp => _critDamage,
        DamageUp => _damage,
        PierceUp => _pierce,
        ProjectileSpeedUp => _projectileSpeed,
        RangeUp => _range,
        ShotCountUp => _shotCount,
        BulletSplitUp => _shotCount, // todo icons for all below
        ChainLightningUp => _shotCount,
        ExplodeOnKillUp => _shotCount,
        HealthRegenUp => _shotCount,
        WeaponUnlock<Shotgun> => _shotCount,
        WeaponUnlock<DamageAura> => _shotCount,
        WeaponUnlock<SniperRifle> => _shotCount,

        null => null,
        _ => throw new ArgumentOutOfRangeException(nameof(node)),
    };
}

internal static class Pluralization
{
    extension(int value)
    {
        internal string HeartLabel() => $"{value} {(value == 1 ? "heart" : "hearts")}";
        internal string EnemiesLabel() => value == 1 ? "enemy" : "enemies";
    }
}