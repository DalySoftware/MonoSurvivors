using System.Collections.Generic;
using Gameplay.Levelling.PowerUps;

namespace Gameplay.Levelling.SphereGrid;

public sealed class GridTemplate
{
    public int RootId { get; init; }

    public List<NodeTemplate> Nodes { get; init; } = [];
}

public sealed class NodeTemplate
{
    public int Id { get; init; }

    public PowerUpCategory Category { get; init; }
    public NodeRarity Rarity { get; init; }

    // Only store outward edges
    public Dictionary<EdgeDirection, int> Neighbours { get; init; } = new();
}