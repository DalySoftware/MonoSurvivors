using Gameplay.Combat.Weapons;

namespace Gameplay.Levelling.PowerUps.Player;

// ReSharper disable once UnusedTypeParameter
// T is effectively used as a marker
public record WeaponUnlock<T> : IPlayerPowerUp
    where T : IWeapon;