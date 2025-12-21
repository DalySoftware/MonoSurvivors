using Gameplay.Combat.Weapons;
using Gameplay.Entities;

namespace Gameplay.Levelling.PowerUps.Player;

// ReSharper disable once UnusedTypeParameter
// T is effectively used as a marker
public record WeaponUnlock<T> : WeaponUnlock where T : IWeapon
{
    public override void Apply(PlayerCharacter player)
    {
        var weapon = player.WeaponFactory.Create<T>(player);
        player.WeaponBelt.AddWeapon(weapon);
    }
}

public abstract record WeaponUnlock : IPlayerPowerUp
{
    public abstract void Apply(PlayerCharacter player);
}