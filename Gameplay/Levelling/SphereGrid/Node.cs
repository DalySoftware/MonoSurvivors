using System.Collections.Generic;
using Gameplay.Levelling.PowerUps;

namespace Gameplay.Levelling.SphereGrid;

public class Node(IPowerUp? powerUp, NodeRarity rarity)
{
    private readonly Dictionary<EdgeDirection, Node> _neighbours = new();
    public NodeRarity Rarity { get; } = rarity;
    public IPowerUp? PowerUp { get; } = powerUp;

    public IReadOnlyDictionary<EdgeDirection, Node> Neighbours => _neighbours;

    public Node? GetNeighbour(EdgeDirection direction) => _neighbours.GetValueOrDefault(direction);

    public void SetNeighbour(EdgeDirection direction, Node? node)
    {
        if (node == null)
        {
            _neighbours.Remove(direction);
            return;
        }

        _neighbours[direction] = node;
    }
}