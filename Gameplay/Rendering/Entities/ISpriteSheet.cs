using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public interface ISpriteSheet
{
    public Texture2D Texture { get; }
    public Rectangle GetFrameRectangle(IFrame frame);
}