using Gameplay.Behaviour;

namespace Gameplay.CollisionDetection;

internal class CircleCollider(IHasPosition owner, float radius) : ICollider
{
    public float CollisionRadius { get; } = radius;
    public Vector2 Position => owner.Position;
}