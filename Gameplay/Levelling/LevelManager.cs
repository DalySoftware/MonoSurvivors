using System;
using Gameplay.Entities;

namespace Gameplay.Levelling;

public class LevelManager
{
    private readonly LevelCalculator _levelCalculator;
    private readonly Action<int> _onLevelUp;
    private readonly PlayerCharacter _player;

    private int _lastSeenPlayerLevel = 1;

    public LevelManager(PlayerCharacter player, IGlobalCommands globalCommands)
    {
        _player = player;
        _onLevelUp = globalCommands.OnLevelUp;

        const float baseRequirement = 10f;
        const float growthFactor = 1.3f;
        _levelCalculator = new LevelCalculator(baseRequirement, growthFactor);

        _player.OnExperienceGain -= OnExperienceGain;
        _player.OnExperienceGain += OnExperienceGain;
    }

    public int Level => _levelCalculator.GetLevel(_player.Experience);

    public float ExperienceSinceLastLevel => _player.Experience - (float)_levelCalculator.TotalExperienceToReach(Level);
    public float ExperienceToNextLevel => (float)_levelCalculator.ExtraExperienceToLevelUpFrom(Level);

    private void OnExperienceGain(object? _, PlayerCharacter __)
    {
        if (_lastSeenPlayerLevel == Level) return;

        _onLevelUp(Level - _lastSeenPlayerLevel);
        _lastSeenPlayerLevel = Level;
    }
}