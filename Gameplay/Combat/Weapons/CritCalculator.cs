using System;
using Gameplay.Audio;

namespace Gameplay.Combat.Weapons;

public class CritCalculator(IAudioPlayer audio)
{
    internal const float BaseCritDamageMultiplier = 2f;
    private readonly static Random Random = new();

    /// <summary>
    ///     Calculate critical damage from some base amount. Plays audio on crit
    ///     <remarks>Do not use this if <paramref name="baseDamage" /> comes from somewhere which already applied crits</remarks>
    /// </summary>
    internal float CalculateCritDamage(
        float baseDamage,
        WeaponBeltStats stats,
        float critChanceMultiplier = 1f)
    {
        var totalCrits = CalculateCrits(stats, critChanceMultiplier); // handles audio

        // Additive stacking for multi-crits
        var bonusPerCrit = stats.CritDamageMultiplier - 1f;

        var finalMultiplier = 1f + totalCrits * bonusPerCrit;
        return baseDamage * finalMultiplier;
    }

    /// <summary>
    ///     Check if an action should crit. Allows multi crits if crit chance > 100%. Plays audio on crit
    ///     <remarks>
    ///         Only use this if the crit has a non damage boost effects. For damage use <see cref="CalculateCritDamage" />.
    ///     </remarks>
    ///     <returns>Number of crits</returns>
    /// </summary>
    internal int CalculateCrits(WeaponBeltStats stats, float critChanceMultiplier = 1f)
    {
        var critChance = stats.CritChance * critChanceMultiplier;

        var guaranteedCrits = (int)MathF.Floor(critChance);

        var fractionalChance = critChance - guaranteedCrits;
        var extraCrit = Random.NextDouble() < fractionalChance ? 1 : 0;

        var total = guaranteedCrits + extraCrit;
        if (total > 0) audio.Play(SoundEffectTypes.Crit);

        return total;
    }
}