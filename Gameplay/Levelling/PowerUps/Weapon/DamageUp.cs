using Gameplay.Levelling.PowerUps.Player;

namespace Gameplay.Levelling.PowerUps.Weapon;

/// <param name="Value">Proportional increase. Accumulated additively.</param>
/// <example>0.5f makes player deal 150% of base damage. </example>
/// <example>0.1f and 0.2f make player deal 130% of base damage.</example>
public record DamageUp(float Value) : IWeaponPowerUp;