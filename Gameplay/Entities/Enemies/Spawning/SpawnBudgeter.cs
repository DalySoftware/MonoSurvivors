using System;
using System.Collections.Generic;
using System.Linq;

namespace Gameplay.Entities.Enemies.Spawning;

public sealed class SpawnBudgeter
{
    public float Budget { get; private set; }

    public void AddBudget(float amount) => Budget += amount;

    public IEnumerable<SpawnEntry> Spend(IReadOnlyList<SpawnEntry> entries)
    {
        if (entries.Count == 0)
            yield break;

        var cheapest = entries.Min(e => e.Cost);

        while (Budget >= cheapest)
        {
            var entry = PickWeighted(entries);

            if (entry.Cost > Budget)
                yield break;

            Budget -= entry.Cost;
            yield return entry;
        }
    }

    private static SpawnEntry PickWeighted(IReadOnlyList<SpawnEntry> entries)
    {
        var totalWeight = entries.Sum(e => e.Weight);

        var roll = Random.Shared.NextSingle() * totalWeight;

        foreach (var entry in entries)
        {
            roll -= entry.Weight;
            if (roll <= 0f)
                return entry;
        }

        return entries[^1];
    }
}