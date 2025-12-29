using Gameplay.Behaviour;

namespace Gameplay.CollisionDetection;

public interface ICollider : IHasPosition
{
    // Doesn't need to be exact for non circular colliders
    public float ApproximateRadius { get; }
}

public interface IHasCollider
{
    ICollider Collider { get; }
}