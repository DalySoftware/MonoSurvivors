using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Entities.Effects;
using Gameplay.Entities.Enemies;

namespace Gameplay.Combat.Weapons.OnHitEffects;

public sealed class ChainLightningOnHit(
    IEntityFinder entityFinder,
    ISpawnEntity spawnEntity,
    IAudioPlayer audio,
    CritCalculator critCalculator)
    : IOnHitEffect
{
    private const float DamageFalloff = 0.6f;
    private const float MaxJumpDistance = 200f;

    private const int ChainsPerProc = 3;
    private const float BaseDamage = 8f;

    public int Priority => 1;

    public void Apply(IHitContext context)
    {
        var stats = context.Owner.WeaponBelt.Stats;

        var remainingChains = ChainsPerProc * CalculateNumberOfProcs(stats);

        var hitEnemies = new HashSet<EnemyBase> { context.Enemy };
        var currentEnemy = context.Enemy;

        // Damage doesn't crit, proc count does
        var damage = BaseDamage * stats.DamageMultiplier;

        while (remainingChains-- > 0)
        {
            var next = entityFinder
                .EnemiesCloseTo(currentEnemy.Position, MaxJumpDistance)
                .Where(e => !hitEnemies.Contains(e))
                .OrderBy(e => Vector2.DistanceSquared(
                    e.Position, currentEnemy.Position))
                .FirstOrDefault();

            if (next == null)
                break;

            SpawnVisual(currentEnemy.Position, next.Position);
            PlayAudio();
            next.TakeDamage(context.Owner, damage);

            hitEnemies.Add(next);
            currentEnemy = next;
            damage *= DamageFalloff;
        }
    }

    private int CalculateNumberOfProcs(WeaponBeltStats stats)
    {
        var chance = MathF.Max(0f, stats.ChainLightningChance);

        var guaranteedProcs = (int)MathF.Floor(chance);

        var fractionalChance = chance - guaranteedProcs;
        var extraFromFraction = Random.Shared.NextSingle() < fractionalChance ? 1 : 0;
        var baseProcs = guaranteedProcs + extraFromFraction;

        if (baseProcs == 0)
            return 0;

        var crits = critCalculator.CalculateCrits(stats);

        // Each crit adds +100% more chains
        return baseProcs * (1 + crits);
    }

    private void SpawnVisual(Vector2 from, Vector2 to) => spawnEntity.Spawn(new LightningArc(from, to));

    private void PlayAudio() => audio.Play(SoundEffectTypes.Lightning);
}