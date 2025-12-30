using System.Collections.Generic;
using Gameplay.Behaviour;

namespace Gameplay.CollisionDetection;

public interface ICollider : IHasPosition
{
    // Doesn't need to be exact for non circular colliders
    public float ApproximateRadius { get; }
}

public interface IHasColliders
{
    IEnumerable<ICollider> Colliders { get; }
}