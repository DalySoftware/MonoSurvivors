namespace Characters;

public class PlayerCharacter
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }

    public void UpdatePosition(GameTime gameTime) => Position += Velocity;

    private float speed = 0f;
    public void DirectionInput(UnitVector2 input)
    {
        Velocity = (Vector2)input * speed;
    }
}