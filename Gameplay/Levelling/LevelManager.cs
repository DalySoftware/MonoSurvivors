using System;
using Gameplay.Entities;

namespace Gameplay.Levelling;

public class LevelManager
{
    private readonly LevelCalculator _levelCalculator;
    private readonly Action _onLevelUp;
    private readonly PlayerCharacter _player;

    private int _lastSeenPlayerLevel = 1;

    public LevelManager(PlayerCharacter player, Action onLevelUp)
    {
        _player = player;
        _onLevelUp = onLevelUp;

        const float baseRequirement = 10f;
        const float growthFactor = 1.3f;
        _levelCalculator = new LevelCalculator(baseRequirement, growthFactor);

        _player.OnExperienceGain -= OnExperienceGain;
        _player.OnExperienceGain += OnExperienceGain;
    }

    public int Level => _levelCalculator.GetLevel(_player.Experience);

    public float ExperienceToNextLevel => (float)_levelCalculator.ExtraExperienceToLevelUpFrom(Level);

    private void OnExperienceGain(object? _, PlayerCharacter __)
    {
        if (_lastSeenPlayerLevel == Level) return;

        _onLevelUp();
        _lastSeenPlayerLevel = Level;
    }
}