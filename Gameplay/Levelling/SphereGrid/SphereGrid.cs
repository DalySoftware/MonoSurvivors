using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Levelling.PowerUps;

namespace Gameplay.Levelling.SphereGrid;

public class SphereGrid
{
    private readonly HashSet<Node> _nodes = [];
    private readonly HashSet<Node> _unlockedNodes = [];

    internal SphereGrid(Node root)
    {
        DiscoverAndAddNodes(root);
        Root = root;
        _unlockedNodes.Add(root);
    }

    public IReadOnlySet<Node> Nodes => _nodes;
    public Node Root { get; private init; }
    public Action<IPowerUp> OnUnlock { get; init; } = _ => { };
    public int AvailablePoints { get; private set; } = 0;

    public IEnumerable<Node> Unlockable => _nodes.Where(CanUnlock);

    public void AddSkillPoints(int points) => AvailablePoints += points;

    public bool CanUnlock(Node node) =>
        AvailablePoints >= node.Cost &&
        !IsUnlocked(node) &&
        _unlockedNodes.Any(n => n.Neighbours.Values.Contains(node));

    public bool IsUnlocked(Node node) => _unlockedNodes.Contains(node);

    public void Unlock(Node node)
    {
        if (!CanUnlock(node))
            return;

        _unlockedNodes.Add(node);
        AvailablePoints -= node.Cost;
        if (node.PowerUp is not null)
            OnUnlock(node.PowerUp);
    }

    /// <summary>
    ///     Discovers all nodes in the graph. Adds them to <see cref="_nodes" />. Maps reverse edges.
    /// </summary>
    private void DiscoverAndAddNodes(Node root)
    {
        var toVisit = new Stack<Node>();
        var visited = new HashSet<Node>();
        toVisit.Push(root);

        while (toVisit.TryPop(out var node))
        {
            if (!visited.Add(node))
                continue;

            _nodes.Add(node);

            foreach (var (dir, neighbour) in node.Neighbours)
            {
                toVisit.Push(neighbour);

                // Assign reverse copy of edge
                neighbour.SetNeighbour(dir.Opposite(), node);
            }
        }
    }
}