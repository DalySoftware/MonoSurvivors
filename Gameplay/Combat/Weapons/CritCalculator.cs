using System;

namespace Gameplay.Combat.Weapons;

internal static class CritCalculator
{
    internal const float BaseCritDamageMultiplier = 2f;
    private readonly static Random Random = new();

    internal static float CalculateCrit(float baseDamage, WeaponBeltStats stats, float critChanceMultiplier = 1f)
    {
        var isCrit = Random.NextDouble() < stats.CritChance * critChanceMultiplier;
        var critDamage = baseDamage * stats.CritDamageMultiplier;
        return isCrit ? critDamage : baseDamage;
    }
}