using System;
using System.Collections.Generic;
using Gameplay.Levelling.PowerUps;

namespace Gameplay.Levelling.SphereGrid;

internal class PowerUpRandomizer
{
    internal readonly static Dictionary<PowerUpCategory, Func<NodeRarity, Node>[]> Factories =
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
                NodeFactory.SniperRifleUnlock,
                NodeFactory.DamageAuraUnlock,
            ],
        };
    private readonly Random _random = new();
    private readonly List<Func<NodeRarity, Node>> _remainingWeaponUnlocks = [..Factories[PowerUpCategory.WeaponUnlock]];

    public Node Pick(PowerUpCategory category, NodeRarity rarity)
    {
        if (category == PowerUpCategory.WeaponUnlock)
        {
            if (_remainingWeaponUnlocks.Count == 0)
                throw new InvalidOperationException("All weapon unlocks already used.");

            var index = _random.Next(_remainingWeaponUnlocks.Count);
            var factory = _remainingWeaponUnlocks[index];
            _remainingWeaponUnlocks.RemoveAt(index);

            return factory(rarity);
        }

        if (!Factories.TryGetValue(category, out var options) || options.Length == 0)
            throw new InvalidOperationException($"No power-ups registered for category {category}");

        return options[_random.Next(options.Length)](rarity);
    }
}