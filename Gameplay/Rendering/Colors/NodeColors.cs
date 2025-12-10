using System;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;
using Gameplay.Levelling.SphereGrid;

namespace Gameplay.Rendering.Colors;

public static class NodeColors
{
    public static Color BaseColor(this Node node) => node.PowerUp switch
    {
        MaxHealthUp => new OklchColor(0.7f, 0.16f, 23).ToColor(),
        LifeStealUp => new OklchColor(0.35f, 0.16f, 26).ToColor(),
        SpeedUp => new OklchColor(0.7f, 0.16f, 80).ToColor(),
        AttackSpeedUp => new OklchColor(0.8f, 0.20f, 115).ToColor(),
        ShotCountUp => new OklchColor(0.8f, 0.20f, 125).ToColor(),
        PickupRadiusUp => new OklchColor(0.7f, 0.16f, 160).ToColor(),
        RangeUp => new OklchColor(0.7f, 0.16f, 215).ToColor(),
        DamageUp => new OklchColor(0.7f, 0.16f, 295).ToColor(),
        ExperienceUp => new OklchColor(0.7f, 0.16f, 340).ToColor(),
        null => Color.Gold,
        _ => throw new ArgumentOutOfRangeException(nameof(node))
    };
}