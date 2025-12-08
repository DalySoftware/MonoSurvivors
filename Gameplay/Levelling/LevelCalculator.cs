using System;

namespace Gameplay.Levelling;

/// <param name="baseRequirement">Experience requirement to reach level 2</param>
/// <param name="growthFactor">
///     How much experience is required to level up from one level to the next.
///     <example><c>1.5f</c> means we require 50% more than the previous level</example>
/// </param>
internal class LevelCalculator(float baseRequirement, float growthFactor)
{
    internal int GetLevel(float totalExperience) 
        // would need updating if we ever need negative exp
        => (int)Math.Pow(totalExperience / baseRequirement, 1f / growthFactor) + 1; 

    internal double TotalExperienceToReach(int level) 
        => baseRequirement * Math.Pow(level - 1, growthFactor);

    internal double ExtraExperienceToLevelUpFrom(int level) 
        => TotalExperienceToReach(level + 1) - TotalExperienceToReach(level);
}