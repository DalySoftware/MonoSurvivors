namespace Gameplay.Levelling.PowerUps.Player;

/// <param name="Value">Proportional increase. Accumulated additively.</param>
public record ExperienceUp(int Value) : IPlayerPowerUp;