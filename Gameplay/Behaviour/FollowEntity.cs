using Gameplay.Utilities;

namespace Gameplay.Behaviour;

internal class FollowEntity(IHasPosition owner, IHasPosition target, float speed)
{
    internal Vector2 CalculateVelocity()
    {
        var direction = target.Position - owner.Position;
        return (Vector2)new UnitVector2(direction) * speed;
    }
}