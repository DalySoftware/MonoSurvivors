using System;
using Gameplay.Entities;

namespace Gameplay.Levelling;

/// <summary>
/// XP → Level progression derived from a single source of truth: total XP.
/// Level and thresholds are computed on demand using a simple deterministic growth curve.
/// </summary>
public class LevelManager
{
    private readonly PlayerCharacter _player;
    private readonly Action _onLevelUp;

    public LevelManager(PlayerCharacter player, Action onLevelUp)
    {
        _player = player;
        _onLevelUp = onLevelUp;
        
        _player.OnExperienceGain -= OnExperienceGain;
        _player.OnExperienceGain += OnExperienceGain;
    }
    /// <summary>
    /// Experience requirement to reach level 2
    /// </summary>
    private const float BaseRequirement = 10f;
    
    /// <summary>
    /// How much experience is required to level up from one level to the next.
    /// </summary>
    /// <example><c>1.5f</c> means we require 50% more than the previous level</example>
    private const float GrowthFactor = 1.5f;

    public int Level => GetLevel();

    public float ExperienceToNextLevel => (float)ExtraExperienceToLevelUpFrom(Level);

    private int _lastSeenPlayerLevel = 1;
    private void OnExperienceGain(object? _, PlayerCharacter __)
    {
        if (_lastSeenPlayerLevel == Level) return;
        
        _onLevelUp();
        _lastSeenPlayerLevel = Level;
    }

    /// <summary>
    /// Current player level. No fractions
    /// </summary>
    private int GetLevel() 
        // would need updating if we ever need negative exp
        => (int)Math.Pow(_player.Experience / BaseRequirement, 1f / GrowthFactor) + 1; 

    private static double TotalExperienceToReach(int level) 
        => BaseRequirement * Math.Pow(level - 1, GrowthFactor);

    private static double ExtraExperienceToLevelUpFrom(int level) 
        => TotalExperienceToReach(level + 1) - TotalExperienceToReach(level);
}
