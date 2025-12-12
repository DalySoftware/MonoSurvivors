using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public interface ISpriteSheet
{
    public Texture2D Texture(ContentManager content);
    public Rectangle GetFrameRectangle(IFrame frame);
}