using Gameplay.Levelling;

namespace Tests;

// Invariant tests to ensure our calculations are consistent
public class LevelCalculatorTests
{
    private readonly LevelCalculator _levelCalculator = new(10f, 1.5f);

    [Test]
    [Arguments(0f)]
    [Arguments(10f)]
    [Arguments(100f)]
    [Arguments(1000f)]
    public async Task GetLevel_TotalExperienceToReach_Invariant(float experience)
    {
        var level = _levelCalculator.GetLevel(experience);
        var experienceToReachLevel = _levelCalculator.TotalExperienceToReach(level);
        await Assert.That(experienceToReachLevel).IsLessThanOrEqualTo(experience);
    }

    [Test]
    [Arguments(1, 2)]
    [Arguments(1, 20)]
    [Arguments(10, 15)]
    public async Task TotalExperienceToReach_ExtraExperienceToLevelUpFrom_Invariant(int levelOne, int levelTwo)
    {
        var levelOneExperience = _levelCalculator.TotalExperienceToReach(levelOne);
        var levelTwoExperience = _levelCalculator.TotalExperienceToReach(levelTwo);

        double summedExperience = 0;

        for (var i = levelOne; i < levelTwo; i++)
            summedExperience += _levelCalculator.ExtraExperienceToLevelUpFrom(i);

        await Assert.That(summedExperience).IsEqualTo(levelTwoExperience - levelOneExperience);
    }
}