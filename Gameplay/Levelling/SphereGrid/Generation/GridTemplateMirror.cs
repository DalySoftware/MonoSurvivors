using System.Linq;

namespace Gameplay.Levelling.SphereGrid.Generation;

internal class GridTemplateMirror
{
    internal static GridTemplate Mirror(GridTemplate template) => new()
    {
        RootId = template.RootId,
        Nodes = template.Nodes
            .Select(MirrorNode)
            .ToList(),
    };

    private static NodeTemplate MirrorNode(NodeTemplate node) => new()
    {
        Id = node.Id,
        Category = node.Category,
        Rarity = node.Rarity,
        Neighbours = node.Neighbours.ToDictionary(
            kvp => kvp.Key.Mirror(),
            kvp => kvp.Value),
    };
}

internal static class EdgeDirectionMirrorExtensions
{
    internal static EdgeDirection Mirror(this EdgeDirection direction) => direction switch
    {
        EdgeDirection.TopLeft => EdgeDirection.TopRight,
        EdgeDirection.TopRight => EdgeDirection.TopLeft,

        EdgeDirection.MiddleLeft => EdgeDirection.MiddleRight,
        EdgeDirection.MiddleRight => EdgeDirection.MiddleLeft,

        EdgeDirection.BottomLeft => EdgeDirection.BottomRight,
        EdgeDirection.BottomRight => EdgeDirection.BottomLeft,

        _ => direction,
    };
}