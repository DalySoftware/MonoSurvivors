using Gameplay.Behaviour;

namespace Gameplay.CollisionDetection;

internal class RectangleCollider(IHasPosition owner, float width, float height) : ICollider
{
    public float Width { get; } = width;
    public float Height { get; } = height;
    public Vector2 Position => owner.Position;
}