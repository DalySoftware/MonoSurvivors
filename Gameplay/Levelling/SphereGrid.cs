using System;
using System.Collections.Generic;
using System.Linq;

namespace Gameplay.Levelling;

public class Node
{
    public int Cost { get; init; } = 1;
    
    private readonly Dictionary<EdgeDirection, Node> _neighbours = new();

    public Node? GetNeighbour(EdgeDirection direction) => _neighbours.GetValueOrDefault(direction);

    public void SetNeighbour(EdgeDirection direction, Node node) => _neighbours[direction] = node;

    public IReadOnlyDictionary<EdgeDirection, Node> Neighbours => _neighbours;
}

public class SphereGrid
{
    private readonly HashSet<Node> _nodes = [];
    private readonly HashSet<Node> _unlockedNodes = [];
    private int _availablePoints = 0;
    
    public IReadOnlySet<Node> Nodes => _nodes;
    
    private void AddNode(Node node) => _nodes.Add(node);
    
    public void AddSkillPoints(int points) => _availablePoints += points;

    public bool CanUnlock(Node node) =>
        _availablePoints >= node.Cost &&
        _unlockedNodes.Any(n => n.Neighbours.Values.Contains(node));
    
    
    public bool IsUnlocked(Node node) => _unlockedNodes.Contains(node);

    public void Unlock(Node node)
    {
        if (!CanUnlock(node))
            return;
        
        _unlockedNodes.Add(node);
        _availablePoints -= node.Cost;
    }
    
    
    private void UnlockRoot(Node rootNode) => _unlockedNodes.Add(rootNode);
    
    /// <summary>
    /// Discovers all nodes in the graph. Adds them to <see cref="_nodes"/>. Maps reverse paths.
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

            AddNode(node);

            foreach (var (dir, neighbour) in node.Neighbours)
            {
                toVisit.Push(neighbour);

                // Assign reverse copy of edge
                neighbour.SetNeighbour(dir.Opposite(), node);
            }
        }
    }

    public static SphereGrid CreateDemo()
    {
        var grid = new SphereGrid();

        // Strength path (right)
        var strKey = new Node();
        var str2 = new Node();
        str2.SetNeighbour(EdgeDirection.MiddleRight, strKey);
        var str1 = new Node();
        str1.SetNeighbour(EdgeDirection.MiddleRight, str2);

        // Agility path (up-right)
        var agi2 = new Node();
        var agi1 = new Node();
        agi1.SetNeighbour(EdgeDirection.TopRight, agi2);

        // Defence path (down-right)
        var def2 = new Node();
        var def1 = new Node();
        def1.SetNeighbour(EdgeDirection.BottomRight, def2);

        var root = new Node { Cost = 0};
        root.SetNeighbour(EdgeDirection.TopRight, agi1);
        root.SetNeighbour(EdgeDirection.MiddleRight, str1);
        root.SetNeighbour(EdgeDirection.BottomRight, def1);

        grid.DiscoverAndAddNodes(root);
        grid.UnlockRoot(root);
        
        return grid;
    }
}

public enum EdgeDirection
{
    TopLeft,
    TopRight,
    MiddleLeft,
    MiddleRight,
    BottomLeft,
    BottomRight,
}

internal static class EdgeDirectionExtensions
{
    internal static EdgeDirection Opposite(this EdgeDirection direction) => direction switch
    {
        EdgeDirection.TopLeft      => EdgeDirection.BottomRight,
        EdgeDirection.TopRight     => EdgeDirection.BottomLeft,
        EdgeDirection.MiddleLeft   => EdgeDirection.MiddleRight,
        EdgeDirection.MiddleRight  => EdgeDirection.MiddleLeft,
        EdgeDirection.BottomLeft   => EdgeDirection.TopRight,
        EdgeDirection.BottomRight  => EdgeDirection.TopLeft,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
}

