using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities;
using Gameplay.Levelling.SphereGrid;

namespace Gameplay.Levelling;

public class LevelManager
{
    private readonly LevelCalculator _levelCalculator;
    private readonly PlayerCharacter _player;
    private readonly IGlobalCommands _globalCommands;
    private readonly SphereGrid.SphereGrid _sphereGrid;

    private int _lastSeenPlayerLevel = 1;

    private HashSet<Node> _lastSeenUnlockables = [];

    public LevelManager(PlayerCharacter player, IGlobalCommands globalCommands, SphereGrid.SphereGrid sphereGrid)
    {
        _player = player;
        _globalCommands = globalCommands;
        _sphereGrid = sphereGrid;

        const float baseRequirement = 10f;
        const float growthFactor = 1.26f;
        _levelCalculator = new LevelCalculator(baseRequirement, growthFactor);

        _player.OnExperienceGain -= OnExperienceGain;
        _player.OnExperienceGain += OnExperienceGain;
    }

    private int Level => _levelCalculator.GetLevel(_player.Experience);

    public float ExperienceSinceLastLevel => _player.Experience - (float)_levelCalculator.TotalExperienceToReach(Level);
    public float ExperienceToNextLevel => (float)_levelCalculator.ExtraExperienceToLevelUpFrom(Level);

    private void OnExperienceGain(object? _, PlayerCharacter __)
    {
        if (_lastSeenPlayerLevel == Level) return;

        OnLevelUp(Level - _lastSeenPlayerLevel);
        _lastSeenPlayerLevel = Level;
    }

    private void OnLevelUp(int levelsGained)
    {
        _sphereGrid.AddSkillPoints(levelsGained);
        var unlockables = _sphereGrid.Unlockable.ToHashSet();
        var anythingHasChanged = !unlockables.SetEquals(_lastSeenUnlockables);
        if (unlockables.Count > 0 && anythingHasChanged)
            _globalCommands.ShowSphereGrid();

        _lastSeenUnlockables = unlockables;
    }
}