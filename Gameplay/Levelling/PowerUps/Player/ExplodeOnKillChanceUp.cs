namespace Gameplay.Levelling.PowerUps.Player;

/// <param name="Value">Proportional chance. Accumulated additively.</param>
/// <example>0.5f makes player dodge 50% of the time. </example>
/// <example>0.1f and 0.2f makes player dodge 30% of the time.</example>
public record ExplodeOnKillChanceUp(float Value) : IPlayerPowerUp;