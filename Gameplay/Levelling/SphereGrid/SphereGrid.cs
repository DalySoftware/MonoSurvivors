using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities;
using Gameplay.Levelling.PowerUps.Player;

namespace Gameplay.Levelling.SphereGrid;

public class Node(IPlayerPowerUp? powerUp, int cost)
{
    public int Cost { get; } = cost;
    public IPlayerPowerUp? PowerUp { get; } = powerUp;
    
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
    public Node Root { get; private init; }
    public Action<IPlayerPowerUp> OnUnlock { get; init; } = _ => { };

    private SphereGrid(Node root)
    {
        DiscoverAndAddNodes(root);
        Root = root;
        _unlockedNodes.Add(root);
    }

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
        if (node.PowerUp is not null)
            OnUnlock(node.PowerUp);
    }

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

            _nodes.Add(node);

            foreach (var (dir, neighbour) in node.Neighbours)
            {
                toVisit.Push(neighbour);

                // Assign reverse copy of edge
                neighbour.SetNeighbour(dir.Opposite(), node);
            }
        }
    }

    public static SphereGrid Create(PlayerCharacter player)
    {
        // Strength path (right)
        var strengthUp = new StrengthUp();
        var strKey = new Node(strengthUp, 1);
        var str2 = new Node(strengthUp, 1);
        str2.SetNeighbour(EdgeDirection.MiddleRight, strKey);
        var str1 = new Node(strengthUp, 1);
        str1.SetNeighbour(EdgeDirection.MiddleRight, str2);

        // Agility path (up-right)
        var speedUp = new SpeedUp(0.2f);
        var agi2 = new Node(speedUp, 1);
        var agi1 = new Node(speedUp, 1);
        agi1.SetNeighbour(EdgeDirection.TopRight, agi2);

        // Defence path (down-right)
        var maxHealthUp = new MaxHealthUp(2);
        var def2 = new Node(maxHealthUp, 1);
        var def1 = new Node(maxHealthUp, 1);
        def1.SetNeighbour(EdgeDirection.BottomRight, def2);

        var root = new Node(null, 0);
        root.SetNeighbour(EdgeDirection.TopRight, agi1);
        root.SetNeighbour(EdgeDirection.MiddleRight, str1);
        root.SetNeighbour(EdgeDirection.BottomRight, def1);

        return new SphereGrid(root)
        {
            OnUnlock = player.Add
        };
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

