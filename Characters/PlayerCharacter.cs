using Characters.Utilities;

namespace Characters;

public class PlayerCharacter(Vector2 position) : Character(position)
{
    private const float Speed = 0.5f;

    public void DirectionInput(UnitVector2 input)
    {
        Velocity = (Vector2)input * Speed;
    }
}