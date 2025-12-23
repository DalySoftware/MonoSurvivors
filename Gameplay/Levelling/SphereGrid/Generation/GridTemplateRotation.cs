using System.Linq;

namespace Gameplay.Levelling.SphereGrid.Generation;

internal static class GridTemplateRotation
{
    internal static GridTemplate Rotate(GridTemplate template, int clockwiseSteps)
    {
        if (clockwiseSteps % 6 == 0)
            return template;

        return new GridTemplate
        {
            RootId = template.RootId,
            Nodes = template.Nodes
                .Select(n => RotateNode(n, clockwiseSteps))
                .ToList(),
        };
    }

    private static NodeTemplate RotateNode(NodeTemplate node, int steps) => new()
    {
        Id = node.Id,
        Category = node.Category,
        Rarity = node.Rarity,
        Neighbours = node.Neighbours.ToDictionary(
            kvp => kvp.Key.RotateClockwise(steps),
            kvp => kvp.Value),
    };
}