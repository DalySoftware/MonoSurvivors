namespace Gameplay.Levelling.PowerUps.Weapon;

/// <param name="Value">Proportional chance. Accumulated additively.</param>
/// <example>0.5f makes player crit 50% of the time. </example>
/// <example>0.1f and 0.2f makes player crit 30% of the time.</example>
public record CritChanceUp(float Value) : IWeaponPowerUp;