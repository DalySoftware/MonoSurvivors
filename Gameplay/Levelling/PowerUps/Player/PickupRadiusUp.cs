namespace Gameplay.Levelling.PowerUps.Player;

/// <param name="Value">Proportional increase. Accumulated additively.</param>
public record PickupRadiusUp(float Value) : IPlayerPowerUp;