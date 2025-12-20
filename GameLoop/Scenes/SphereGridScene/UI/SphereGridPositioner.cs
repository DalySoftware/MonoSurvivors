using System;
using System.Collections.Generic;
using Gameplay.Levelling.SphereGrid;
using Microsoft.Xna.Framework;

namespace GameLoop.Scenes.SphereGridScene.UI;

/// <param name="hexRadius">How large to make the hexagons, ie how far between nodes</param>
internal class SphereGridPositioner(Node root)
{
    internal const float HexRadius = 160f;

    private readonly NodePositionMap? _cached = null;

    internal NodePositionMap NodePositions() => _cached ?? CalculateNodePositions();

    private NodePositionMap CalculateNodePositions()
    {
        var positions = new Dictionary<Node, Vector2>
        {
            // Place root node at origin
            [root] = Vector2.Zero,
        };

        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();
        queue.Enqueue(root);
        visited.Add(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentPos = positions[current];

            foreach (var (direction, neighbor) in current.Neighbours)
            {
                if (visited.Contains(neighbor)) continue;

                // Calculate hex offset based on direction
                var offset = GetHexOffset(direction);
                positions[neighbor] = currentPos + offset;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return new NodePositionMap(positions);
    }

    private static Vector2 GetHexOffset(EdgeDirection direction)
    {
        // Hexagon positioning with equal 60° angles
        var angle = direction switch
        {
            EdgeDirection.MiddleRight => 0f,
            EdgeDirection.TopRight => MathF.PI / 3f,
            EdgeDirection.TopLeft => 2f * MathF.PI / 3f,
            EdgeDirection.MiddleLeft => MathF.PI,
            EdgeDirection.BottomLeft => 4f * MathF.PI / 3f,
            EdgeDirection.BottomRight => 5f * MathF.PI / 3f,
            _ => 0f,
        };

        return new Vector2(
            HexRadius * MathF.Cos(angle),
            -HexRadius * MathF.Sin(angle) // Negate Y for screen coordinates
        );
    }
}

internal class NodePositionMap(IReadOnlyDictionary<Node, Vector2> nodePositions)
{
    internal IReadOnlyDictionary<Node, Vector2> Positions => nodePositions;
}