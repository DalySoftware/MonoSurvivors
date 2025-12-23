using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.SphereGrid.Generation;

namespace Tests;

public class SphereGridGenerationTests
{
    [Test]
    public async Task SphereGrid_HasEnoughWeaponUnlocks_ForPowerUpRandomizer()
    {
        var weaponUnlocks = PowerUpRandomizer.ByCategory[PowerUpCategory.WeaponUnlock];

        var sphereGrid = GridFactory.CreateRandom(_ => { });
        await Assert.That(sphereGrid.Nodes)
            .Count(n => n.PowerUp?.GetType().IsGenericType == true &&
                        n.PowerUp.GetType().GetGenericTypeDefinition() == typeof(WeaponUnlock<>))
            .IsEqualTo(weaponUnlocks.Count);
    }

    [Test]
    public async Task RandomSphereGrid_OnlyIncludesEachWeaponOnce()
    {
        var weapons = PowerUpCatalog.Categories.Where(kvp => kvp.Value == PowerUpCategory.WeaponUnlock)
            .Select(kvp => kvp.Key);

        var grid = GridFactory.CreateRandom(_ => { });

        await Assert.That(weapons)
            .All(w => grid.Nodes.Count(n => n.PowerUp?.GetType() == w) == 1);
    }
}