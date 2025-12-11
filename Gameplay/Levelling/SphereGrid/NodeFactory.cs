using System;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;

namespace Gameplay.Levelling.SphereGrid;

internal static class NodeFactory
{
    internal static Node DamageUp(int nodeLevel) =>
        new(new DamageUp(nodeLevel * 0.25f), nodeLevel, nodeLevel);

    internal static Node SpeedUp(int nodeLevel) =>
        new(new SpeedUp(nodeLevel * 0.2f), nodeLevel, nodeLevel);

    internal static Node MaxHealthUp(int nodeLevel) =>
        new(new MaxHealthUp(nodeLevel * 2), nodeLevel, nodeLevel);

    internal static Node AttackSpeedUp(int nodeLevel) =>
        new(new AttackSpeedUp(nodeLevel * 0.2f), nodeLevel, nodeLevel);

    internal static Node PickupRadiusUp(int nodeLevel) =>
        new(new PickupRadiusUp(nodeLevel * 0.3f), nodeLevel, nodeLevel);

    internal static Node RangeUp(int nodeLevel) =>
        new(new RangeUp(nodeLevel * 0.3f), nodeLevel, nodeLevel);

    internal static Node LifeStealUp(int nodeLevel) =>
        new(new LifeStealUp(nodeLevel), nodeLevel, nodeLevel * 2);

    internal static Node ExperienceUp(int nodeLevel) =>
        new(new ExperienceUp(nodeLevel * 0.2f), nodeLevel, nodeLevel);

    internal static Node CritChanceUp(int nodeLevel) =>
        new(new CritChanceUp(nodeLevel * 0.05f), nodeLevel, nodeLevel);

    internal static Node CritDamageUp(int nodeLevel) =>
        new(new CritDamageUp(nodeLevel * 0.1f), nodeLevel, nodeLevel);

    internal static Node PierceUp(int nodeLevel) =>
        new(new PierceUp(nodeLevel), nodeLevel, PierceCost(nodeLevel));

    private static int PierceCost(int nodeLevel) => nodeLevel switch
    {
        2 => 3,
        1 => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(nodeLevel))
    };

    internal static Node ProjectileSpeedUp(int nodeLevel) =>
        new(new ProjectileSpeedUp(nodeLevel * 0.2f), nodeLevel, nodeLevel);

    internal static Node ShotCountUp(int nodeLevel) =>
        new(new ShotCountUp(nodeLevel), nodeLevel, ShotCountCost(nodeLevel));

    private static int ShotCountCost(int nodeLevel) => nodeLevel switch
    {
        2 => 5,
        1 => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(nodeLevel))
    };
}