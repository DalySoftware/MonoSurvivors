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
        [typeof(DodgeChanceUp)] = PowerUpCategory.Speed,
        [typeof(ProjectileSpeedUp)] = PowerUpCategory.Utility,

        // Utility
        [typeof(PickupRadiusUp)] = PowerUpCategory.Utility,
        [typeof(ExperienceUp)] = PowerUpCategory.Utility,
        [typeof(RangeUp)] = PowerUpCategory.Utility,

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
            AttackSpeedUp => "Increase Attack Speed",
            BulletSplitUp => "Increase Bullet Split",
            ChainLightningUp => "Increase Chain Lightning chance",
            CritChanceUp => "Increase Critical Hit Chance",
            CritDamageUp => "Increase Critical Hit Damage",
            DamageUp => "Increase Damage",
            DodgeChanceUp => "Increase DodgeChance",
            ExperienceUp => "Increase Experience Multiplier",
            ExplodeOnKillUp => "Increase on kill explosion",
            GridVisionUp => "Increase Grid Vision",
            HealthRegenUp => "Increase Health Regen",
            LifeStealUp => "Increase Life Steal",
            MaxHealthUp => "Increase Max Health",
            PickupRadiusUp => "Increase Pickup Radius",
            PierceUp => "Pierce more enemies",
            ProjectileSpeedUp => "Increase Projectile Speed",
            RangeUp => "Increase Range",
            ShotCountUp => "Increase Shot Count",
            SpeedUp => "Increase Speed",
            WeaponUnlock<Shotgun> => "Unlock the Shotgun",
            WeaponUnlock<SniperRifle> => "Unlock the Sniper Rifle",
            WeaponUnlock<DamageAura> => "Unlock a damaging aura",
            _ => throw new ArgumentOutOfRangeException(nameof(powerUp)),
        };

        public string Description() => powerUp switch
        {
            AttackSpeedUp attackSpeedUp => $"Increase Attack Speed by {attackSpeedUp.Value:P0}",
            BulletSplitUp => "Increase Bullet Split",
            ChainLightningUp => "Increase chance to trigger chain lightning on hit",
            CritChanceUp critChanceUp => $"Increase Critical Hit Chance by {critChanceUp.Value:P0}",
            CritDamageUp critDamageUp => $"Increase Critical Hit Damage by {critDamageUp.Value:P0}",
            DamageUp damageUp => $"Increase Damage by {damageUp.Value:P0}",
            DodgeChanceUp dodgeChangeUp => $"Increase Dodge Chance by {dodgeChangeUp.Value:P0}",
            ExperienceUp experienceUp => $"Increase Experience Multiplier by {experienceUp.Value:P0}",
            ExplodeOnKillUp => "Increase explosion size on kill",
            GridVisionUp gridVisionUp => $"Increase sphere grid vision range by {gridVisionUp.Value:P0}",
            HealthRegenUp => "Increase Health Regen",
            LifeStealUp => "Increase Life Steal",
            MaxHealthUp maxHealthUp => $"Increase Max Health by {(maxHealthUp.Value / 2).HeartLabel()}",
            PickupRadiusUp pickupRadiusUp => $"Increase Pickup Radius by {pickupRadiusUp.Value:P0}",
            PierceUp pierceUp => $"Projectiles pierce {pierceUp.Value} more {pierceUp.Value.EnemiesLabel()}",
            ProjectileSpeedUp projectileSpeedUp => $"Increase Projectile Speed by {projectileSpeedUp.Value:P0}",
            RangeUp rangeUp => $"Increase Range by {rangeUp.Value:P0}",
            ShotCountUp shotCountUp => $"Fire {shotCountUp.ExtraShots} extra shots",
            SpeedUp speedUp => $"Increase Speed by {speedUp.Value:P0}",
            WeaponUnlock<Shotgun> => "Unlock an extra weapon. The shotgun fires bullets in a spread",
            WeaponUnlock<SniperRifle> => "Unlock an extra weapon. The sniper rifle fires high damage shots",
            WeaponUnlock<DamageAura> => "Unlock an extra weapon. You emit an aura which damages all nearby enemies",
            _ => throw new ArgumentOutOfRangeException(nameof(powerUp)),
        };
    }

    private static class Colors
    {
        private readonly static Color Health = new OklchColor(0.7f, 0.16f, 23).ToColor();
        private readonly static Color Speed = new OklchColor(0.7f, 0.16f, 80).ToColor();
        private readonly static Color Utility = new OklchColor(0.7f, 0.16f, 160).ToColor();
        private readonly static Color Special = new OklchColor(0.7f, 0.16f, 215).ToColor();
        private readonly static Color Damage = new OklchColor(0.7f, 0.16f, 295).ToColor();
        private readonly static Color Crit = new OklchColor(0.7f, 0.16f, 340).ToColor();
        private readonly static Color DamageEffects = new OklchColor(0.8f, 0.20f, 125).ToColor();

        internal readonly static IReadOnlyDictionary<PowerUpCategory, Color> ByCategory =
            new Dictionary<PowerUpCategory, Color>
            {
                { PowerUpCategory.Damage, Damage },
                { PowerUpCategory.DamageEffects, DamageEffects },
                { PowerUpCategory.Health, Health },
                { PowerUpCategory.Speed, Speed },
                { PowerUpCategory.Utility, Utility },
                { PowerUpCategory.Crit, Crit },
                { PowerUpCategory.WeaponUnlock, Special },
            };
    }

    extension(IPowerUp? powerUp)
    {
        public Color BaseColor()
        {
            if (powerUp is null) return Color.Gold;

            var category = Categories[powerUp.GetType()];
            return Colors.ByCategory[category];
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
        AttackSpeedUp => _attackSpeed,
        CritChanceUp => _critChance,
        CritDamageUp => _critDamage,
        DamageUp => _damage,

        ExperienceUp => _experience,
        LifeStealUp => _lifeSteal,
        MaxHealthUp => _maxHealth,
        PickupRadiusUp => _pickupRadius,
        PierceUp => _pierce,
        ProjectileSpeedUp => _projectileSpeed,
        RangeUp => _range,
        ShotCountUp => _shotCount,
        SpeedUp => _speed,
        BulletSplitUp => _shotCount, // todo icons for all below
        ChainLightningUp => _shotCount,
        DodgeChanceUp => _shotCount,
        ExplodeOnKillUp => _shotCount,
        GridVisionUp => _shotCount,
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