using System;

namespace Gameplay.Combat.Weapons;

internal static class CritCalculator
{
    internal const float BaseCritDamageMultiplier = 2f;
    private readonly static Random Random = new();

    internal static float CalculateCrit(
        float baseDamage,
        WeaponBeltStats stats,
        float critChanceMultiplier = 1f)
    {
        var critChance = stats.CritChance * critChanceMultiplier;

        var guaranteedCrits = (int)MathF.Floor(critChance);

        var fractionalChance = critChance - guaranteedCrits;
        var extraCrit = Random.NextDouble() < fractionalChance ? 1 : 0;

        var totalCrits = guaranteedCrits + extraCrit;

        if (totalCrits <= 0)
            return baseDamage;

        // Additive stacking for multi-crits
        var bonusPerCrit = stats.CritDamageMultiplier - 1f;

        var finalMultiplier = 1f + totalCrits * bonusPerCrit;

        return baseDamage * finalMultiplier;
    }
}