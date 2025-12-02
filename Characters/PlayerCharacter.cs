namespace Characters;

public class PlayerCharacter(Vector2 position)
{
    public Vector2 Position { get; private set; } = position;
    private Vector2 Velocity { get; set; } = Vector2.Zero;

    public void UpdatePosition(GameTime gameTime) => Position += Velocity * gameTime.ElapsedGameTime.Milliseconds;

    private const float Speed = 0.5f;

    public void DirectionInput(UnitVector2 input)
    {
        Velocity = (Vector2)input * Speed;
    }
}