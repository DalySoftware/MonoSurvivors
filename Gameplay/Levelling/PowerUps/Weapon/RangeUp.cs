namespace Gameplay.Levelling.PowerUps.Weapon;

/// <param name="Value">Proportional increase. Accumulated additively.</param>
public record RangeUp(float Value) : IWeaponPowerUp;