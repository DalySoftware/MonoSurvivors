using System;
using System.Collections.Generic;
using Gameplay.Entities.Enemies;

namespace Gameplay.Stats;

public class StatsCounter
{
    public float DamageDealt { get; private set; }
    public int DamageTaken { get; private set; }
    public int HealingReceived { get; private set; }
    public float ExperienceGained { get; private set; }
    public Dictionary<KillKey, int> Kills { get; } = new();

    internal void TrackDamageDealt(float amount) => DamageDealt += amount;
    internal void TrackDamageTaken(int amount) => DamageTaken += amount;
    internal void TrackHealing(int amount) => HealingReceived += amount;
    internal void TrackExperienceGained(float amount) => ExperienceGained += amount;

    internal void TrackKill(EnemyBase enemy)
    {
        var key = new KillKey(enemy.GetType(), enemy.IsElite);
        Kills.Increment(key);
    }

    public readonly record struct KillKey(Type EnemyType, bool IsElite);
}

internal static class DictionaryExtensions
{
    internal static void Increment<TKey>(this Dictionary<TKey, int> dict, TKey key)
        where TKey : notnull =>
        dict[key] = dict.GetValueOrDefault(key) + 1;
}