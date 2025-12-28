namespace Gameplay.Levelling.PowerUps.Weapon;

/// <summary>
///     Add chance to fire an extra shot
/// </summary>
/// <param name="Value">Proportional chance. Accumulated additively.</param>
/// <example>0.5f fires 50% of the time. </example>
/// <example>0.1f and 0.2f fires 30% of the time.</example>
/// <example>1.5f fires a second shot 100% of the time, and a third shot 50% of the time.</example>
public record ExtraShotChanceUp(float Value) : IWeaponPowerUp;