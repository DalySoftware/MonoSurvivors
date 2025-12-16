using Gameplay.Combat.Weapons;

namespace Gameplay.Levelling.PowerUps.Player;

public record WeaponUnlock<T> : IPlayerPowerUp
    where T : IWeapon;