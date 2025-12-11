using System;

namespace Gameplay.Combat.Weapons;

internal static class CritCalculator
{
    internal const float BaseCritDamageMultiplier = 2f;
    private readonly static Random Random = new();

    internal static float CalculateDamage(float baseDamage, float critChance, float? critDamageMultiplier)
    {
        var isCrit = Random.NextDouble() < critChance;
        var critDamage = baseDamage * (critDamageMultiplier ?? BaseCritDamageMultiplier);
        return isCrit ? critDamage : baseDamage;
    }
}