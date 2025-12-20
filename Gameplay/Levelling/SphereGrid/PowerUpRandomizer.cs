using System;
using System.Collections.Generic;
using Gameplay.Levelling.PowerUps;

namespace Gameplay.Levelling.SphereGrid;

internal static class PowerUpRandomizer
{
    private readonly static Random Rng = new();

    private readonly static Dictionary<PowerUpCategory, Func<NodeRarity, Node>[]> Factories =
        new()
        {
            [PowerUpCategory.Damage] =
            [
                NodeFactory.DamageUp,
                NodeFactory.AttackSpeedUp,
            ],

            [PowerUpCategory.DamageEffects] =
            [
                NodeFactory.ShotCountUp,
                NodeFactory.PierceUp,
                NodeFactory.BulletSplitUp,
                NodeFactory.ExplodeOnKillUp,
                NodeFactory.ChainLightningUp,
            ],

            [PowerUpCategory.Health] =
            [
                NodeFactory.MaxHealthUp,
                NodeFactory.HealthRegenUp,
                NodeFactory.LifeStealUp,
            ],

            [PowerUpCategory.Speed] =
            [
                NodeFactory.SpeedUp,
                NodeFactory.DodgeChanceUp,
            ],

            [PowerUpCategory.Utility] =
            [
                NodeFactory.PickupRadiusUp,
                NodeFactory.ExperienceUp,
                NodeFactory.RangeUp,
                NodeFactory.ProjectileSpeedUp,
            ],

            [PowerUpCategory.Crit] =
            [
                NodeFactory.CritChanceUp,
                NodeFactory.CritDamageUp,
            ],

            [PowerUpCategory.WeaponUnlock] =
            [
                NodeFactory.ShotgunUnlock,
                NodeFactory.SniperUnlock,
                NodeFactory.DamageAuraUnlock,
            ],
        };

    internal static Node Pick(PowerUpCategory category, NodeRarity rarity)
    {
        if (!Factories.TryGetValue(category, out var options) || options.Length == 0)
            throw new InvalidOperationException($"No power-ups registered for category {category}");

        var factory = options[Rng.Next(options.Length)];
        return factory(rarity);
    }
}