using System;
using ContentLibrary;
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
    extension(IPowerUp powerUp)
    {
        public string Title() => powerUp switch
        {
            MaxHealthUp => "Increase Max Health",
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
            WeaponUnlock<Shotgun> => "Unlock the Shotgun",
            ChainLightningUp => "Increase Chain Lightning chance",
            _ => throw new ArgumentOutOfRangeException(nameof(powerUp)),
        };
        public string Description() => powerUp switch
        {
            MaxHealthUp maxHealthUp => $"Increase Max Health by {(maxHealthUp.Value / 2).HeartLabel()}",
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
            WeaponUnlock<Shotgun> => "Unlock an extra weapon. The shotgun fires bullets in a spread",
            ChainLightningUp => "Increase chance to trigger chain lightning on hit",
            _ => throw new ArgumentOutOfRangeException(nameof(powerUp)),
        };
    }

    extension(IPowerUp? powerUp)
    {
        public Color BaseColor()
        {
            // Categories
            var healthColor = new OklchColor(0.7f, 0.16f, 23).ToColor();
            var speedColor = new OklchColor(0.7f, 0.16f, 80).ToColor();
            var utilityColor = new OklchColor(0.7f, 0.16f, 160).ToColor();
            var specialColor = new OklchColor(0.7f, 0.16f, 215).ToColor();
            var damageColor = new OklchColor(0.7f, 0.16f, 295).ToColor();
            var critColor = new OklchColor(0.7f, 0.16f, 340).ToColor();
            var damageEffectsColor = new OklchColor(0.8f, 0.20f, 125).ToColor();

            return powerUp switch
            {
                DamageUp or AttackSpeedUp => damageColor,
                ShotCountUp or PierceUp or BulletSplitUp or ExplodeOnKillUp or ChainLightningUp => damageEffectsColor,
                MaxHealthUp or LifeStealUp => healthColor,
                SpeedUp => speedColor,
                PickupRadiusUp or ExperienceUp or RangeUp or ProjectileSpeedUp => utilityColor,
                CritChanceUp or CritDamageUp => critColor,
                WeaponUnlock<Shotgun> => specialColor,
                null => Color.Gold,
                _ => throw new ArgumentOutOfRangeException(nameof(powerUp)),
            };
        }
    }
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
        BulletSplitUp => _shotCount, // todo
        ExplodeOnKillUp => _shotCount, // todo
        WeaponUnlock<Shotgun> => _shotCount, // todo
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