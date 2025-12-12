using Gameplay.Behaviour;

namespace Gameplay.CollisionDetection;

public interface ICollider : IHasPosition;

public interface IHasCollider
{
    ICollider Collider { get; }
}