namespace Gameplay.Levelling.PowerUps.Player;

/// <param name="Bullets">Number of bullets to spawn</param>
public record ExplodeOnKillUp(int Bullets) : IPlayerPowerUp;