using System;
using Gameplay.Combat.Weapons.AreaOfEffect;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;

namespace Gameplay.Levelling.SphereGrid;

internal static class NodeFactory
{
    private static Scaling LowValueScaling(NodeRarity rarity) => rarity switch
    {
        NodeRarity.Legendary => new Scaling(3, 5),
        NodeRarity.Rare => new Scaling(2, 3),
        _ => new Scaling(1, 1),
    };

    private static Scaling MediumValueScaling(NodeRarity rarity) => rarity switch
    {
        NodeRarity.Legendary => new Scaling(5, 3),
        NodeRarity.Rare => new Scaling(3, 2),
        _ => new Scaling(2, 1),
    };

    private static Scaling WeaponUnlockScaling(NodeRarity rarity) =>
        rarity is NodeRarity.Legendary
            ? new Scaling(5, 0)
            : throw new Exception("This category is only available for legendary nodes");

    internal static Scaling ScalingFor(Type powerUpType, NodeRarity rarity) =>
        PowerUpCatalog.Categories[powerUpType] switch
        {
            PowerUpCategory.Damage or
                PowerUpCategory.Health or
                PowerUpCategory.Speed or
                PowerUpCategory.Utility or
                PowerUpCategory.Crit => LowValueScaling(rarity),

            PowerUpCategory.DamageEffects => MediumValueScaling(rarity),

            PowerUpCategory.WeaponUnlock => WeaponUnlockScaling(rarity),

            _ => throw new ArgumentOutOfRangeException(nameof(powerUpType)),
        };

    private static Node CreateNode<T>(NodeRarity rarity, Func<float, T> constructor)
        where T : IPowerUp
    {
        var scaleFactor = ScalingFor(typeof(T), rarity).ScaleFactor;
        return new Node(constructor(scaleFactor), rarity);
    }

    internal static Node AttackSpeedUp(NodeRarity rarity) => CreateNode(rarity, s => new AttackSpeedUp(s * 0.2f));
    internal static Node BulletSplitUp(NodeRarity rarity) => CreateNode(rarity, s => new BulletSplitUp((int)s * 2));
    internal static Node ChainLightningUp(NodeRarity rarity) => CreateNode(rarity, s => new ChainLightningUp(s * 0.1f));
    internal static Node CritChanceUp(NodeRarity rarity) => CreateNode(rarity, s => new CritChanceUp(s * 0.05f));
    internal static Node CritDamageUp(NodeRarity rarity) => CreateNode(rarity, s => new CritDamageUp(s * 0.1f));
    internal static Node DamageUp(NodeRarity rarity) => CreateNode(rarity, s => new DamageUp(s * 0.25f));
    internal static Node DodgeChanceUp(NodeRarity rarity) => CreateNode(rarity, s => new DodgeChanceUp(s * 0.03f));
    internal static Node ExperienceUp(NodeRarity rarity) => CreateNode(rarity, s => new ExperienceUp(s * 0.2f));
    internal static Node ExplodeOnKillUp(NodeRarity rarity) => CreateNode(rarity, s => new ExplodeOnKillUp((int)s * 3));
    internal static Node GridVisionUp(NodeRarity rarity) => CreateNode(rarity, s => new GridVisionUp(s * 0.1f));
    internal static Node HealthRegenUp(NodeRarity rarity) => CreateNode(rarity, s => new HealthRegenUp((int)s));
    internal static Node LifeStealUp(NodeRarity rarity) => CreateNode(rarity, s => new LifeStealUp((int)s));
    internal static Node MaxHealthUp(NodeRarity rarity) => CreateNode(rarity, s => new MaxHealthUp((int)s * 2));
    internal static Node PickupRadiusUp(NodeRarity rarity) => CreateNode(rarity, s => new PickupRadiusUp(s * 0.3f));
    internal static Node PierceUp(NodeRarity rarity) => CreateNode(rarity, s => new PierceUp((int)s));
    internal static Node ProjectileSpeedUp(NodeRarity rarity) =>
        CreateNode(rarity, s => new ProjectileSpeedUp(s * 0.2f));
    internal static Node RangeUp(NodeRarity rarity) => CreateNode(rarity, s => new RangeUp(s * 0.3f));
    internal static Node ShotCountUp(NodeRarity rarity) => CreateNode(rarity, s => new ShotCountUp((int)s));
    internal static Node SpeedUp(NodeRarity rarity) => CreateNode(rarity, s => new SpeedUp(s * 0.05f));

    internal static Node BouncingGunUnlock(NodeRarity rarity) => new(new WeaponUnlock<BouncingGun>(), rarity);
    internal static Node DamageAuraUnlock(NodeRarity rarity) => new(new WeaponUnlock<DamageAura>(), rarity);
    internal static Node ShotgunUnlock(NodeRarity rarity) => new(new WeaponUnlock<Shotgun>(), rarity);
    internal static Node SniperRifleUnlock(NodeRarity rarity) => new(new WeaponUnlock<SniperRifle>(), rarity);

    public record Scaling(int Cost, int ScaleFactor);
}

public static class NodeExtensions
{
    extension(Node node)
    {
        public int Cost =>
            node.PowerUp is null
                ? 0
                : NodeFactory.ScalingFor(node.PowerUp.GetType(), node.Rarity).Cost;
    }
}