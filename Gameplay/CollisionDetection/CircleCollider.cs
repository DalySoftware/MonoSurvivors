using Gameplay.Behaviour;

namespace Gameplay.CollisionDetection;

internal class CircleCollider(IHasPosition owner, float radius) : ICollider
{
    internal float CollisionRadius { get; set; } = radius;
    public Vector2 Position => owner.Position;
    public float ApproximateRadius => CollisionRadius;
}