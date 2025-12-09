using System.Collections.Generic;
using Gameplay.Entities;
using Gameplay.Levelling.PowerUps.Weapon;

namespace Gameplay.Combat.Weapons;

public class WeaponBelt : IEntity
{
    private readonly List<IWeapon> _weapons = [];
    private readonly List<IWeaponPowerUp> _powerUps = [];

    public void Update(GameTime gameTime) => _weapons.ForEach(w => w.Update(gameTime, _powerUps));

    public void AddWeapon(IWeapon weapon) => _weapons.Add(weapon);
    public void AddPowerUp(IWeaponPowerUp powerUp) => _powerUps.Add(powerUp);
}