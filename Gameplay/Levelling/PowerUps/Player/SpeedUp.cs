namespace Gameplay.Levelling.PowerUps.Player;

/// <param name="Value">Proportional increase. Accumulated additively.</param>
/// <example>0.5f makes player 150% of base speed. </example>
/// <example>0.1f and 0.2f makes player 130% of base speed.</example>
public record SpeedUp(float Value) : IPlayerPowerUp;