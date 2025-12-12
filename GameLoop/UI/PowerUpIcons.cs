using System;
using ContentLibrary;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;
using Gameplay.Levelling.SphereGrid;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

internal class PowerUpIcons(ContentManager content)
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

    internal Texture2D? IconFor(Node node) => node.PowerUp switch
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
        null => null,
        _ => throw new ArgumentOutOfRangeException(nameof(node))
    };
}