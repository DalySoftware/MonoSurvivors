using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.SphereGrid;

namespace Tests;

public class PowerUpRandomizerTests
{
    [Test]
    public async Task PowerUpRandomizer_HasAFactoryRegistered_ForEveryPowerUp()
    {
        var powerUpWithCategories = PowerUpCatalog.Categories;

        foreach (var (powerUpType, category) in powerUpWithCategories)
            await Assert.That(PowerUpRandomizer.Factories[category])
                .Any(f => f.Method.Name == ComparableName(powerUpType));
    }

    private static string ComparableName(Type type) => type switch
    {
        { IsGenericType: true } when type.GetGenericTypeDefinition() == typeof(WeaponUnlock<>) =>
            $"{type.GetGenericArguments()[0].Name}Unlock",
        _ => type.Name,
    };

    [Test]
    public async Task SphereGrid_HasEnoughWeaponUnlocks_ForPowerUpRandomizer()
    {
        var weaponUnlocks = PowerUpRandomizer.Factories[PowerUpCategory.WeaponUnlock];

        var sphereGrid = GridFactory.CreateRandom(_ => { });
        await Assert.That(sphereGrid.Nodes)
            .Count(n => n.PowerUp?.GetType().IsGenericType == true &&
                        n.PowerUp.GetType().GetGenericTypeDefinition() == typeof(WeaponUnlock<>))
            .IsEqualTo(weaponUnlocks.Length);
    }
}