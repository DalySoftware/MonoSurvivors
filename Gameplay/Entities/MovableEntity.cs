using Gameplay.Behaviour;

namespace Gameplay.Entities;

public abstract class MovableEntity(Vector2 position) : IEntity, IHasPosition
{
    protected Vector2 Velocity { get; set; } = Vector2.Zero;

    public virtual void Update(GameTime gameTime) => Position += Velocity * gameTime.ElapsedGameTime.Milliseconds;
    public bool MarkedForDeletion { get; protected set; }
    public Vector2 Position { get; private set; } = position;
}