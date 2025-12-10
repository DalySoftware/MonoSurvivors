using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;

namespace Gameplay.Levelling.SphereGrid;

public class Node(IPowerUp? powerUp, int cost)
{
    private readonly Dictionary<EdgeDirection, Node> _neighbours = new();
    public int Cost { get; } = cost;
    public IPowerUp? PowerUp { get; } = powerUp;

    public IReadOnlyDictionary<EdgeDirection, Node> Neighbours => _neighbours;

    public Node? GetNeighbour(EdgeDirection direction) => _neighbours.GetValueOrDefault(direction);

    public void SetNeighbour(EdgeDirection direction, Node node) => _neighbours[direction] = node;
}

public class SphereGrid
{
    private readonly HashSet<Node> _nodes = [];
    private readonly HashSet<Node> _unlockedNodes = [];
    private int _availablePoints = 0;

    private SphereGrid(Node root)
    {
        DiscoverAndAddNodes(root);
        Root = root;
        _unlockedNodes.Add(root);
    }

    public IReadOnlySet<Node> Nodes => _nodes;
    public Node Root { get; private init; }
    public Action<IPowerUp> OnUnlock { get; init; } = _ => { };
    public bool IsComplete => !_nodes.Except(_unlockedNodes).Any();

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
    ///     Discovers all nodes in the graph. Adds them to <see cref="_nodes" />. Maps reverse paths.
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
        Node DamageUp(int nodeLevel) => new(new DamageUp(nodeLevel * 0.25f), nodeLevel);
        Node SpeedUp(int nodeLevel) => new(new SpeedUp(nodeLevel * 0.2f), nodeLevel);
        Node MaxHealthUp(int nodeLevel) => new(new MaxHealthUp(nodeLevel * 2), nodeLevel);
        Node AttackSpeedUp(int nodeLevel) => new(new AttackSpeedUp(nodeLevel * 0.2f), nodeLevel);
        Node PickupRadiusUp(int nodeLevel) => new(new PickupRadiusUp(nodeLevel * 0.3f), nodeLevel);
        Node RangeUp(int nodeLevel) => new(new RangeUp(nodeLevel * 0.5f), nodeLevel);

        Node ShotCountUp(int nodeLevel) => nodeLevel switch
        {
            2 => new Node(new ShotCountUp(2), 5),
            1 => new Node(new ShotCountUp(1), 3),
            _ => throw new ArgumentOutOfRangeException(nameof(nodeLevel))
        };

        // Damage (right)
        var strKey = DamageUp(2);
        var dmg2 = DamageUp(1);
        dmg2.SetNeighbour(EdgeDirection.MiddleRight, strKey);
        var dmg1 = DamageUp(1);
        dmg1.SetNeighbour(EdgeDirection.MiddleRight, dmg2);

        // Speed (up-right)
        var spd2 = SpeedUp(1);
        var spd1 = SpeedUp(1);
        spd1.SetNeighbour(EdgeDirection.TopRight, spd2);

        // Max HP (down-right)
        var hp2 = MaxHealthUp(1);
        var hp1 = MaxHealthUp(1);
        hp1.SetNeighbour(EdgeDirection.BottomRight, hp2);

        // Attack Speed (left)
        var extraShotNode = ShotCountUp(1);
        var atkSpd2 = AttackSpeedUp(1);
        atkSpd2.SetNeighbour(EdgeDirection.MiddleLeft, extraShotNode);
        var atkSpd1 = AttackSpeedUp(1);
        atkSpd1.SetNeighbour(EdgeDirection.MiddleLeft, atkSpd2);

        // Pickup Radius (up-left)
        var pickupRadius2 = PickupRadiusUp(1);
        var pickupRadius1 = PickupRadiusUp(1);
        pickupRadius1.SetNeighbour(EdgeDirection.TopLeft, pickupRadius2);

        // Range (down-left)
        var rng2 = RangeUp(1);
        var rng1 = RangeUp(1);
        rng1.SetNeighbour(EdgeDirection.BottomLeft, rng2);

        var root = new Node(null, 0);
        root.SetNeighbour(EdgeDirection.TopRight, spd1);
        root.SetNeighbour(EdgeDirection.MiddleRight, dmg1);
        root.SetNeighbour(EdgeDirection.BottomRight, hp1);
        root.SetNeighbour(EdgeDirection.MiddleLeft, atkSpd1);
        root.SetNeighbour(EdgeDirection.TopLeft, pickupRadius1);
        root.SetNeighbour(EdgeDirection.BottomLeft, rng1);

        return new SphereGrid(root)
        {
            OnUnlock = player.AddPowerUp
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
    BottomRight
}

internal static class EdgeDirectionExtensions
{
    internal static EdgeDirection Opposite(this EdgeDirection direction) => direction switch
    {
        EdgeDirection.TopLeft => EdgeDirection.BottomRight,
        EdgeDirection.TopRight => EdgeDirection.BottomLeft,
        EdgeDirection.MiddleLeft => EdgeDirection.MiddleRight,
        EdgeDirection.MiddleRight => EdgeDirection.MiddleLeft,
        EdgeDirection.BottomLeft => EdgeDirection.TopRight,
        EdgeDirection.BottomRight => EdgeDirection.TopLeft,
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };
}