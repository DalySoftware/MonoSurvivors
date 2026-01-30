namespace Gameplay.Combat.Weapons.OnHitEffects.Visual;

public interface IOnHitVisualEffect
{
    void Apply(IHitContext context);
}