namespace Gameplay.Levelling.PowerUps.Weapon;

/// <summary>
///     Allows bullets to pierce X enemies
/// </summary>
/// <param name="Value">Number of enemies to pierce</param>
public record PierceUp(int Value) : IWeaponPowerUp;