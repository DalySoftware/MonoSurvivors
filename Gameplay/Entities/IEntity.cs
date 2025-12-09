namespace Gameplay.Entities;

public interface IEntity
{
    public bool MarkedForDeletion => false;
    public void Update(GameTime gameTime);
}