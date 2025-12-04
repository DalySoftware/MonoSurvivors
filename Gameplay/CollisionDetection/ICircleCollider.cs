using Gameplay.Behaviour;

namespace Gameplay.CollisionDetection;

internal interface ICircleCollider : IHasPosition
{
    internal float CollisionRadius { get; }
}