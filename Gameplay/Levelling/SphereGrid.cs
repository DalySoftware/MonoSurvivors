using System.Collections.Generic;

namespace Gameplay.Levelling;

public interface ISphereGridNodeEffect
{
    string Description { get; }
    void Apply(object target);
}

public class NoOpEffect : ISphereGridNodeEffect
{
    public string Description => "No effect";
    public void Apply(object target) { }
}


public enum SphereGridNodeType
{
    Small,      // Minor stat boost
    Medium,     // Moderate stat boost
    Large,      // Major stat boost or notable ability
    KeyNode     // Major ability or large stat change
}

public sealed class SphereGridNode
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public SphereGridNodeType Type { get; init; } = SphereGridNodeType.Small;
    public HashSet<string> Neighbors { get; } = [];
    public ISphereGridNodeEffect Effect { get; init; } = new NoOpEffect();
    public Vector2 Position { get; init; } // For rendering
    public int Cost { get; init; } = 1; // Skill points required
}

public class SphereGrid
{
    private readonly Dictionary<string, SphereGridNode> _nodes = new();
    private readonly HashSet<string> _unlocked = [];
    private int _availablePoints;

    public IReadOnlyDictionary<string, SphereGridNode> Nodes => _nodes;
    public IReadOnlyCollection<string> Unlocked => _unlocked;
    public int AvailablePoints => _availablePoints;

    public void AddNode(SphereGridNode node)
    {
        _nodes[node.Id] = node;
    }

    private static void ConnectNodes(SphereGridNode node1, SphereGridNode node2)
    {
        node1.Neighbors.Add(node2.Id);
        node2.Neighbors.Add(node1.Id);
    }

    public void AddSkillPoints(int points)
    {
        _availablePoints += points;
    }

    public bool IsUnlocked(string nodeId) => _unlocked.Contains(nodeId);

    public bool CanUnlock(string nodeId)
    {
        if (!_nodes.ContainsKey(nodeId)) return false;
        if (_unlocked.Contains(nodeId)) return false;

        var node = _nodes[nodeId];
        if (_availablePoints < node.Cost) return false;

        // First node can be unlocked anywhere
        if (_unlocked.Count == 0) return true;

        // Otherwise must be adjacent to an unlocked node
        foreach (var unlockedId in _unlocked)
        {
            if (_nodes[unlockedId].Neighbors.Contains(nodeId))
                return true;
        }

        return false;
    }

    public bool Unlock(string nodeId, object? target = null)
    {
        if (!CanUnlock(nodeId)) return false;

        var node = _nodes[nodeId];
        _availablePoints -= node.Cost;
        _unlocked.Add(nodeId);

        if (target != null)
            node.Effect.Apply(target);

        return true;
    }

    public static SphereGrid CreateDemo()
    {
        var grid = new SphereGrid();

        // Create a small branching path system
        // Start node (center)
        var start = new SphereGridNode
        {
            Id = "start",
            Name = "Origin",
            Type = SphereGridNodeType.KeyNode,
            Position = new Vector2(0, 0),
            Effect = new NoOpEffect()
        };
        grid.AddNode(start);

        // Strength path (right)
        var str1 = new SphereGridNode
        {
            Id = "str1",
            Name = "+STR",
            Type = SphereGridNodeType.Small,
            Position = new Vector2(100, 0),
            Effect = new NoOpEffect()
        };
        var str2 = new SphereGridNode
        {
            Id = "str2",
            Name = "+STR",
            Type = SphereGridNodeType.Small,
            Position = new Vector2(200, 0),
            Effect = new NoOpEffect()
        };
        var strKey = new SphereGridNode
        {
            Id = "str_key",
            Name = "Power Strike",
            Type = SphereGridNodeType.KeyNode,
            Position = new Vector2(300, 0),
            Cost = 3,
            Effect = new NoOpEffect()
        };
        grid.AddNode(str1);
        grid.AddNode(str2);
        grid.AddNode(strKey);

        // Agility path (up-right)
        var agi1 = new SphereGridNode
        {
            Id = "agi1",
            Name = "+AGI",
            Type = SphereGridNodeType.Small,
            Position = new Vector2(70, -70),
            Effect = new NoOpEffect()
        };
        var agi2 = new SphereGridNode
        {
            Id = "agi2",
            Name = "+SPD",
            Type = SphereGridNodeType.Medium,
            Position = new Vector2(140, -140),
            Cost = 2,
            Effect = new NoOpEffect()
        };
        grid.AddNode(agi1);
        grid.AddNode(agi2);

        // Defense path (down-right)
        var def1 = new SphereGridNode
        {
            Id = "def1",
            Name = "+DEF",
            Type = SphereGridNodeType.Small,
            Position = new Vector2(70, 70),
            Effect = new NoOpEffect()
        };
        var def2 = new SphereGridNode
        {
            Id = "def2",
            Name = "+HP",
            Type = SphereGridNodeType.Medium,
            Position = new Vector2(140, 140),
            Cost = 2,
            Effect = new NoOpEffect()
        };
        grid.AddNode(def1);
        grid.AddNode(def2);

        // Connect nodes
        ConnectNodes(start, str1);
        ConnectNodes(str1, str2);
        ConnectNodes(str2, strKey);

        ConnectNodes(start, agi1);
        ConnectNodes(agi1, agi2);

        ConnectNodes(start, def1);
        ConnectNodes(def1, def2);

        // Cross-connection between paths
        ConnectNodes(str1, agi1);
        ConnectNodes(str1, def1);

        return grid;
    }
}
