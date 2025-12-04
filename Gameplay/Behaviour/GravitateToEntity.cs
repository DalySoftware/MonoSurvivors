namespace Gameplay.Behaviour;

public class GravitateToEntity(
    IHasPosition owner,
    IHasPosition target,
    float speedFactor = 1f)
{
    internal Vector2 CalculateVelocity()
    {
        var direction = target.Position - owner.Position;
        var distance = direction.Length();
        direction.Normalize();

        // Gravitational force (inverse square law)
        var speed = 1000f * speedFactor / (distance * distance);
        return direction * speed;
    }
}