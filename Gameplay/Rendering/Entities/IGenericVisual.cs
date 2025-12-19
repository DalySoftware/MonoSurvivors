using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public interface IGenericVisual : IVisual
{
    void Draw(SpriteBatch spriteBatch);
}