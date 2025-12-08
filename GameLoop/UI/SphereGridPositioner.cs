using System;
using System.Collections.Generic;
using Gameplay.Levelling;
using Gameplay.Levelling.SphereGrid;
using Microsoft.Xna.Framework;

namespace GameLoop.UI;

/// <param name="hexRadius">How large to make the hexagons, ie how far between nodes</param>
internal class SphereGridPositioner(SphereGrid grid, float hexRadius)
{
    private readonly Dictionary<Node, Vector2>?  _cached = null;
    
    internal IReadOnlyDictionary<Node, Vector2> NodePositions() => _cached ?? CalculateNodePositions();
    
    private Dictionary<Node, Vector2> CalculateNodePositions()
    {
        var positions = new Dictionary<Node, Vector2>();

        var root = grid.Root;

        // Place root node at origin
        positions[root] = Vector2.Zero;

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
        
        return positions;
    }
    
    private Vector2 GetHexOffset(EdgeDirection direction)
    {
        // Flat-top hexagon positioning
        // Using hex coordinate math where each direction has a specific offset
        var xStep = hexRadius * 1.5f;
        var yStep = hexRadius * (float)Math.Sqrt(3) / 2;

        return direction switch
        {
            EdgeDirection.TopLeft => new Vector2(-xStep, -yStep),
            EdgeDirection.TopRight => new Vector2(xStep, -yStep),
            EdgeDirection.MiddleLeft => new Vector2(-xStep * 2, 0),
            EdgeDirection.MiddleRight => new Vector2(xStep * 2, 0),
            EdgeDirection.BottomLeft => new Vector2(-xStep, yStep),
            EdgeDirection.BottomRight => new Vector2(xStep, yStep),
            _ => Vector2.Zero
        };
    }
}