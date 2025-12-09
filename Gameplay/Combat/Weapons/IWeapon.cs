using System.Collections.Generic;
using Gameplay.Levelling.PowerUps.Weapon;

namespace Gameplay.Combat.Weapons;

public interface IWeapon
{
    public void Update(GameTime gameTime, IReadOnlyCollection<IWeaponPowerUp> powerUps);
}