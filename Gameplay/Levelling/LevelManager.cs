using Gameplay.Audio;
using Gameplay.Entities;

namespace Gameplay.Levelling;

public class LevelManager
{
    private readonly LevelCalculator _levelCalculator;
    private readonly IAudioPlayer _audio;
    private readonly PlayerCharacter _player;
    private readonly SphereGrid.SphereGrid _sphereGrid;

    private int _lastSeenPlayerLevel = 1;

    public LevelManager(PlayerCharacter player, SphereGrid.SphereGrid sphereGrid, LevelCalculator levelCalculator,
        IAudioPlayer audio)
    {
        _player = player;
        _sphereGrid = sphereGrid;
        _levelCalculator = levelCalculator;
        _audio = audio;

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
        _audio.Play(SoundEffectTypes.LevelUp);
    }
}