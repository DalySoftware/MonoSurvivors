namespace Gameplay.Levelling.PowerUps.Weapon;

/// <param name="Value">Proportional increase. Accumulated additively.</param>
/// <example>0f makes player deal 200% of base damage.</example>
/// <example>0.5f makes player deal 250% of base damage. </example>
/// <example>0.1f and 0.2f makes player deal 130% of base damage.</example>
public record CritDamageUp(float Value) : IWeaponPowerUp;