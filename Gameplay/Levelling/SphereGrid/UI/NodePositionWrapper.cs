using System.Collections.Generic;
using Gameplay.Behaviour;

namespace Gameplay.Levelling.SphereGrid.UI;

public readonly record struct NodePositionWrapper(Node Node, IReadOnlyDictionary<Node, Vector2> NodePositions)
    : IHasPosition
{
    public Vector2 Position => NodePositions[Node];
}