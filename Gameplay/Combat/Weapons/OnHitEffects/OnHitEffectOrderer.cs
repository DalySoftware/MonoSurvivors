using System;
using System.Collections.Generic;

namespace Gameplay.Combat.Weapons.OnHitEffects;

public class OnHitEffectOrderer : IComparer<IOnHitEffect>
{
    public int Compare(IOnHitEffect? x, IOnHitEffect? y) => throw new NotImplementedException();
}