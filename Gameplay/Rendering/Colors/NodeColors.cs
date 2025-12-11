using System;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;
using Gameplay.Levelling.SphereGrid;

namespace Gameplay.Rendering.Colors;

public static class NodeColors
{
    public static Color BaseColor(this Node node)
    {
        // Categories
        var healthColor = new OklchColor(0.7f, 0.16f, 23).ToColor();
        var speedColor = new OklchColor(0.7f, 0.16f, 80).ToColor();
        var utilityColor = new OklchColor(0.7f, 0.16f, 160).ToColor();
        var weaponUtilityColor = new OklchColor(0.7f, 0.16f, 215).ToColor();
        var damageColor = new OklchColor(0.7f, 0.16f, 295).ToColor();
        var critColor = new OklchColor(0.7f, 0.16f, 340).ToColor();

        // Unused green-ish
        // new OklchColor(0.8f, 0.20f, 125).ToColor()

        return node.PowerUp switch
        {
            DamageUp => damageColor,
            MaxHealthUp or LifeStealUp => healthColor,
            SpeedUp or AttackSpeedUp => speedColor,
            RangeUp or ShotCountUp or ProjectileSpeedUp or PierceUp => weaponUtilityColor,
            PickupRadiusUp or ExperienceUp => utilityColor,
            CritChanceUp or CritDamageUp => critColor,
            null => Color.Gold,
            _ => throw new ArgumentOutOfRangeException(nameof(node))
        };
    }
}