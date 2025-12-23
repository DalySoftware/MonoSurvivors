using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Levelling.PowerUps;

namespace Gameplay.Levelling.SphereGrid.Generation;

public static class GridFactory
{
    private static SphereGrid CreateFromTemplate(
        GridTemplate template,
        Action<IPowerUp> onUnlock)
    {
        var rotation = Random.Shared.Next(0, 6);
        var rotatedTemplate = GridTemplateRotation.Rotate(template, rotation);

        var nodeMap = CreateNodes(rotatedTemplate);
        WireEdges(rotatedTemplate, nodeMap);
        return new SphereGrid(nodeMap[rotatedTemplate.RootId])
        {
            OnUnlock = onUnlock,
        };
    }

    private static Dictionary<int, Node> CreateNodes(GridTemplate template)
    {
        var map = new Dictionary<int, Node>();

        var randomizer = new PowerUpRandomizer();
        foreach (var nt in template.Nodes.Where(n => n.Id != template.RootId))
        {
            // Pick a concrete power-up for this node
            var node = randomizer.Pick(nt.Category, nt.Rarity);
            map.Add(nt.Id, node);
        }

        var rootNode = new Node(null, NodeRarity.Common);
        map.Add(template.RootId, rootNode);

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