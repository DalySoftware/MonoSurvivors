namespace Gameplay.Levelling.PowerUps.Weapon;

/// <summary>
/// Add more shots to weapons
/// </summary>
public record ShotCountUp(int ExtraShots) : IWeaponPowerUp;