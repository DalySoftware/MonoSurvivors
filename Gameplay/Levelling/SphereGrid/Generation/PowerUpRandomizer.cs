using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;

namespace Gameplay.Levelling.SphereGrid.Generation;

internal class PowerUpRandomizer(Type startingWeapon)
{
    private readonly Random _random = new();

    private readonly List<PowerUpMetaData> _remainingWeaponUnlocks = PowerUpCatalog.PowerUpDefinitions
        .Where(d =>
            d.PowerUpType.IsGenericType &&
            d.PowerUpType.GetGenericTypeDefinition() == typeof(WeaponUnlock<>) &&
            d.PowerUpType.GetGenericArguments()[0] != startingWeapon)
        .ToList();

    internal static Dictionary<PowerUpCategory, List<PowerUpMetaData>> ByCategory { get; } =
        PowerUpCatalog.PowerUpDefinitions
            .GroupBy(d => d.Category)
            .ToDictionary(g => g.Key, g => g.ToList());

    public Node Pick(PowerUpCategory category, NodeRarity rarity)
    {
        if (category == PowerUpCategory.WeaponUnlock)
        {
            if (_remainingWeaponUnlocks.Count == 0)
                throw new InvalidOperationException("All weapon unlocks already used.");

            var index = _random.Next(_remainingWeaponUnlocks.Count);
            var factory = _remainingWeaponUnlocks[index].Factory;
            _remainingWeaponUnlocks.RemoveAt(index);

            return factory(rarity);
        }

        if (!ByCategory.TryGetValue(category, out var options) || options.Count == 0)
            throw new InvalidOperationException($"No power-ups registered for category {category}");

        return options[_random.Next(options.Count)].Factory(rarity);
    }
}