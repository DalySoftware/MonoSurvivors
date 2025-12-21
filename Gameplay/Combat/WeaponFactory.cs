using Autofac;
using Gameplay.Combat.Weapons;
using Gameplay.Entities;

namespace Gameplay.Combat;

public sealed class WeaponFactory(IComponentContext context)
{
    public IWeapon Create<TWeapon>(PlayerCharacter owner)
        where TWeapon : IWeapon =>
        context.Resolve<TWeapon>(new TypedParameter(typeof(PlayerCharacter), owner));
}