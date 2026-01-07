using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.SphereGrid.Generation;

namespace Tests;

[MethodDataSource(nameof(WeaponDescriptors))]
public class SphereGridGenerationTests(WeaponDescriptor startingWeapon)
{
    public static IEnumerable<WeaponDescriptor> WeaponDescriptors => PowerUpCatalog.Weapons;
    [Test]
    public async Task SphereGrid_HasEnoughWeaponUnlocks_ForPowerUpRandomizer()
    {
        var weaponUnlocks = PowerUpRandomizer.ByCategory[PowerUpCategory.WeaponUnlock]
            .Where(t => t.PowerUpType != startingWeapon.Unlock.GetType())
            .ToList();

        var sphereGrid = GridFactory.CreateRandom(_ => { }, startingWeapon);

        await Assert.That(sphereGrid.Nodes)
            .Count(n => n.PowerUp?.GetType().IsGenericType == true &&
                        n.PowerUp.GetType().GetGenericTypeDefinition() == typeof(WeaponUnlock<>))
            .IsEqualTo(weaponUnlocks.Count);
    }

    [Test]
    public async Task RandomSphereGrid_OnlyIncludesEachWeaponOnce()
    {
        var weapons = PowerUpCatalog.Categories
            .Where(kvp =>
                kvp.Value == PowerUpCategory.WeaponUnlock &&
                kvp.Key != startingWeapon.Unlock.GetType())
            .Select(kvp => kvp.Key);

        var grid = GridFactory.CreateRandom(_ => { }, startingWeapon);

        await Assert.That(weapons)
            .All(w => grid.Nodes.Count(n => n.PowerUp?.GetType() == w) == 1);
    }

    [Test]
    public async Task SphereGrid_DoesNotIncludeStartingWeaponUnlock()
    {
        var grid = GridFactory.CreateRandom(_ => { }, startingWeapon);

        await Assert.That(grid.Nodes).DoesNotContain(n => n.PowerUp?.GetType() == startingWeapon.Unlock.GetType());
    }
}