namespace Gameplay.Levelling.PowerUps.Weapon;

/// <param name="Bullets">Number of new bullets on hit. Note, at least 2 will always spawn.</param>
public record BulletSplitUp(int Bullets) : IWeaponPowerUp;