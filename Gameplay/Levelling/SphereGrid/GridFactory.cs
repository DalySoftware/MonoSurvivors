using System;
using System.Collections.Generic;
using Gameplay.Levelling.PowerUps;

namespace Gameplay.Levelling.SphereGrid;

public static class GridFactory
{
    private static SphereGrid CreateFromTemplate(
        GridTemplate template,
        Action<IPowerUp> onUnlock)
    {
        var nodeMap = CreateNodes(template);
        WireEdges(template, nodeMap);
        return new SphereGrid(nodeMap[template.RootId])
        {
            OnUnlock = onUnlock,
        };
    }

    private static Dictionary<int, Node> CreateNodes(GridTemplate template)
    {
        var map = new Dictionary<int, Node>();

        foreach (var nt in template.Nodes)
        {
            // Pick a concrete power-up for this node
            var node = PowerUpRandomizer.Pick(nt.Category, nt.Rarity);
            map.Add(nt.Id, node);
        }

        return map;
    }

    private static void WireEdges(
        GridTemplate template,
        Dictionary<int, Node> nodeMap)
    {
        foreach (var nt in template.Nodes)
        {
            var from = nodeMap[nt.Id];

            foreach (var (direction, targetId) in nt.Neighbours)
            {
                var to = nodeMap[targetId];

                // Prevent double-wiring if template contains both sides
                if (from.GetNeighbour(direction) != null)
                    continue;

                from.SetNeighbour(direction, to);
                to.SetNeighbour(direction.Opposite(), from);
            }
        }
    }

    public static SphereGrid CreateRandom(Action<IPowerUp> onUnlock) =>
        CreateFromTemplate(TemplateFactory.CreateTemplate(), onUnlock);
}