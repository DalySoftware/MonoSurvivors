using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public interface IPrimitiveVisual : IVisual
{
    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer);
}