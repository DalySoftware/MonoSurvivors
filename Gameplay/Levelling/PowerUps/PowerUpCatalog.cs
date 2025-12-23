using System;
using System.Collections.Generic;
using System.Linq;
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
    internal readonly static PowerUpMetaData[] PowerUpDefinitions =
    [
        // Damage
        new(PowerUpCategory.Damage, typeof(DamageUp), NodeFactory.DamageUp),
        new(PowerUpCategory.Damage, typeof(AttackSpeedUp), NodeFactory.AttackSpeedUp),

        // Damage Effects
        new(PowerUpCategory.DamageEffects, typeof(ShotCountUp), NodeFactory.ShotCountUp),
        new(PowerUpCategory.DamageEffects, typeof(PierceUp), NodeFactory.PierceUp),
        new(PowerUpCategory.DamageEffects, typeof(BulletSplitUp), NodeFactory.BulletSplitUp),
        new(PowerUpCategory.DamageEffects, typeof(ExplodeOnKillUp), NodeFactory.ExplodeOnKillUp),
        new(PowerUpCategory.DamageEffects, typeof(ChainLightningUp), NodeFactory.ChainLightningUp),

        // Health
        new(PowerUpCategory.Health, typeof(MaxHealthUp), NodeFactory.MaxHealthUp),
        new(PowerUpCategory.Health, typeof(HealthRegenUp), NodeFactory.HealthRegenUp),
        new(PowerUpCategory.Health, typeof(LifeStealUp), NodeFactory.LifeStealUp),

        // Speed
        new(PowerUpCategory.Speed, typeof(SpeedUp), NodeFactory.SpeedUp),
        new(PowerUpCategory.Speed, typeof(DodgeChanceUp), NodeFactory.DodgeChanceUp),
        new(PowerUpCategory.Utility, typeof(ProjectileSpeedUp), NodeFactory.ProjectileSpeedUp),

        // Utility
        new(PowerUpCategory.Utility, typeof(PickupRadiusUp), NodeFactory.PickupRadiusUp),
        new(PowerUpCategory.Utility, typeof(ExperienceUp), NodeFactory.ExperienceUp),
        new(PowerUpCategory.Utility, typeof(RangeUp), NodeFactory.RangeUp),
        new(PowerUpCategory.Utility, typeof(GridVisionUp), NodeFactory.GridVisionUp),

        // Crit
        new(PowerUpCategory.Crit, typeof(CritChanceUp), NodeFactory.CritChanceUp),
        new(PowerUpCategory.Crit, typeof(CritDamageUp), NodeFactory.CritDamageUp),

        // Weapon unlocks (unique)
        new(PowerUpCategory.WeaponUnlock, typeof(WeaponUnlock<Shotgun>), NodeFactory.ShotgunUnlock),
        new(PowerUpCategory.WeaponUnlock, typeof(WeaponUnlock<SniperRifle>), NodeFactory.SniperRifleUnlock),
        new(PowerUpCategory.WeaponUnlock, typeof(WeaponUnlock<DamageAura>), NodeFactory.DamageAuraUnlock),
        new(PowerUpCategory.WeaponUnlock, typeof(WeaponUnlock<BouncingGun>), NodeFactory.BouncingGunUnlock),
    ];

    internal static Dictionary<Type, PowerUpCategory> Categories { get; } =
        PowerUpDefinitions.ToDictionary(d => d.PowerUpType, d => d.Category);

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
            WeaponUnlock<BouncingGun> => "Unlock the Bouncer",
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
            WeaponUnlock<BouncingGun> => "Unlock an extra weapon. The bouncer's bullets ricochet off enemies",
            WeaponUnlock<DamageAura> => "Unlock an extra weapon. You emit an aura which damages all nearby enemies",
            _ => throw new ArgumentOutOfRangeException(nameof(powerUp)),
        };
    }

    private static class Colors
    {
        private readonly static Color Health = ColorPalette.Red;
        private readonly static Color Speed = ColorPalette.Green;
        private readonly static Color Utility = ColorPalette.Royal;
        private readonly static Color Weapons = ColorPalette.Yellow;
        private readonly static Color Damage = ColorPalette.Pink;
        private readonly static Color Crit = ColorPalette.Orange;
        private readonly static Color DamageEffects = ColorPalette.Violet;

        internal readonly static IReadOnlyDictionary<PowerUpCategory, Color> ByCategory =
            new Dictionary<PowerUpCategory, Color>
            {
                { PowerUpCategory.Damage, Damage },
                { PowerUpCategory.DamageEffects, DamageEffects },
                { PowerUpCategory.Health, Health },
                { PowerUpCategory.Speed, Speed },
                { PowerUpCategory.Utility, Utility },
                { PowerUpCategory.Crit, Crit },
                { PowerUpCategory.WeaponUnlock, Weapons },
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
    private readonly Texture2D _bulletSplit = content.Load<Texture2D>(Paths.Images.PowerUpIcons.BulletSplit);
    private readonly Texture2D _chainLightning = content.Load<Texture2D>(Paths.Images.PowerUpIcons.ChainLightning);
    private readonly Texture2D _critChance = content.Load<Texture2D>(Paths.Images.PowerUpIcons.CritChance);
    private readonly Texture2D _critDamage = content.Load<Texture2D>(Paths.Images.PowerUpIcons.CritDamage);
    private readonly Texture2D _damage = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Damage);
    private readonly Texture2D _dodge = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Dodge);
    private readonly Texture2D _experience = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Experience);
    private readonly Texture2D _explodeOnKill = content.Load<Texture2D>(Paths.Images.PowerUpIcons.ExplodeOnKill);
    private readonly Texture2D _gridVision = content.Load<Texture2D>(Paths.Images.PowerUpIcons.GridVision);
    private readonly Texture2D _lifeSteal = content.Load<Texture2D>(Paths.Images.PowerUpIcons.LifeSteal);
    private readonly Texture2D _healthRegen = content.Load<Texture2D>(Paths.Images.PowerUpIcons.HealthRegen);
    private readonly Texture2D _maxHealth = content.Load<Texture2D>(Paths.Images.PowerUpIcons.MaxHealth);
    private readonly Texture2D _pickupRadius = content.Load<Texture2D>(Paths.Images.PowerUpIcons.PickupRadius);
    private readonly Texture2D _pierce = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Pierce);
    private readonly Texture2D _projectileSpeed = content.Load<Texture2D>(Paths.Images.PowerUpIcons.ProjectileSpeed);
    private readonly Texture2D _range = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Range);
    private readonly Texture2D _shotCount = content.Load<Texture2D>(Paths.Images.PowerUpIcons.ShotCount);
    private readonly Texture2D _speed = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Speed);

    private readonly Texture2D _bouncingGun = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Weapons.BouncingGun);
    private readonly Texture2D _damageAura = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Weapons.DamageAura);
    private readonly Texture2D _shotgun = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Weapons.Shotgun);
    private readonly Texture2D _sniperRifle = content.Load<Texture2D>(Paths.Images.PowerUpIcons.Weapons.SniperRifle);

    public Texture2D? IconFor(Node node) => node.PowerUp switch
    {
        AttackSpeedUp => _attackSpeed,
        BulletSplitUp => _bulletSplit,
        ChainLightningUp => _chainLightning,
        CritChanceUp => _critChance,
        CritDamageUp => _critDamage,
        DamageUp => _damage,
        DodgeChanceUp => _dodge,
        ExperienceUp => _experience,
        ExplodeOnKillUp => _explodeOnKill,
        GridVisionUp => _gridVision,
        HealthRegenUp => _healthRegen,
        LifeStealUp => _lifeSteal,
        MaxHealthUp => _maxHealth,
        PickupRadiusUp => _pickupRadius,
        PierceUp => _pierce,
        ProjectileSpeedUp => _projectileSpeed,
        RangeUp => _range,
        ShotCountUp => _shotCount,
        SpeedUp => _speed,

        WeaponUnlock<Shotgun> => _shotgun,
        WeaponUnlock<DamageAura> => _damageAura,
        WeaponUnlock<BouncingGun> => _bouncingGun,
        WeaponUnlock<SniperRifle> => _sniperRifle,

        null => null,
        _ => throw new ArgumentOutOfRangeException(nameof(node)),
    };
}

internal readonly record struct PowerUpMetaData(
    PowerUpCategory Category,
    Type PowerUpType,
    Func<NodeRarity, Node> Factory
);

internal static class Pluralization
{
    extension(int value)
    {
        internal string HeartLabel() => $"{value} {(value == 1 ? "heart" : "hearts")}";
        internal string EnemiesLabel() => value == 1 ? "enemy" : "enemies";
    }
}