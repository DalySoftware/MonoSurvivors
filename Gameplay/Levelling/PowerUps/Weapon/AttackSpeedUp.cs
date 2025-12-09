namespace Gameplay.Levelling.PowerUps.Weapon;

/// <param name="Value">Proportional increase. Accumulated additively.</param>
/// <example>0.5f makes player attack at 150% of base speed. </example>
/// <example>0.1f and 0.2f make player attack at 130% of base speed.</example>
public record AttackSpeedUp(float Value) : IWeaponPowerUp;