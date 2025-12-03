namespace Characters;

public abstract class Character(Vector2 position)
{
    public Vector2 Position { get; private set; } = position;
    protected Vector2 Velocity { get; set; } = Vector2.Zero;

    public virtual void UpdatePosition(GameTime gameTime) =>
        Position += Velocity * gameTime.ElapsedGameTime.Milliseconds;
}