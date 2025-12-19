using System;

namespace Gameplay.Combat.Weapons;

internal static class CritCalculator
{
    internal const float BaseCritDamageMultiplier = 2f;
    private readonly static Random Random = new();

    internal static float CalculateCrit(float baseDamage, WeaponBeltStats stats)
    {
        var isCrit = Random.NextDouble() < stats.CritChance;
        var critDamage = baseDamage * stats.CritDamageMultiplier;
        return isCrit ? critDamage : baseDamage;
    }
}