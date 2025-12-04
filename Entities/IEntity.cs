namespace Entities;

public interface IEntity
{
    public bool MarkedForDeletion { get; }
    public void Update(GameTime gameTime);
}