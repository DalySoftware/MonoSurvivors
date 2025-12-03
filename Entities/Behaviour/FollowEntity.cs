using Entities.Utilities;

namespace Entities.Behaviour;

public class FollowEntity(IHasPosition owner, IHasPosition target, float speed)
{
    public Vector2 CalculateVelocity()
    {
        var direction = target.Position - owner.Position;
        return (Vector2)new UnitVector2(direction) * speed;
    }
}