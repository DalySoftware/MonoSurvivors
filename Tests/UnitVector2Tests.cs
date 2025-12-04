using Gameplay.Utilities;

namespace Tests;

public class UnitVector2Tests
{
    [Test]
    [Arguments(0f, 0f)]
    [Arguments(1f, 1f)]
    [Arguments(-1f, -1f)]
    [Arguments(-1.0000001f, 0.9999999f)]
    [Arguments(8888888f, 8888888f)]
    [Arguments(-8888888f, -8888888f)]
    public async Task Invariant_XAndYAreAlwaysClampedToUnit(float x, float y)
    {
        var vector = new UnitVector2(x, y);
        await Assert.That(vector.X).IsNotNaN().And.IsBetween(-1f, 1f);
        await Assert.That(vector.Y).IsNotNaN().And.IsBetween(-1f, 1f);
    }
}