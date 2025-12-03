using Entities.Utilities;

namespace Entities;

public class PlayerCharacter(Vector2 position) : MovableEntity(position)
{
    private const float Speed = 0.5f;

    public void DirectionInput(UnitVector2 input) => Velocity = (Vector2)input * Speed;
}