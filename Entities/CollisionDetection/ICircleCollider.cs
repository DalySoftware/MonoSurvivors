namespace Entities.CollisionDetection;

internal interface ICircleCollider : IHasPosition
{
    internal float CollisionRadius { get; }
}