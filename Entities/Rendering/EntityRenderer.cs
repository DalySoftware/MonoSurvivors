using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Entities.Rendering;

internal class EntityRenderer(ContentManager content)
{
    private readonly TextureMap _textureMap = new(content);

    internal void Draw(SpriteBatch spriteBatch, IHasPosition entity)
    {
        var texture = _textureMap.TextureFor(entity);
        spriteBatch.Draw(texture, entity.Position, origin: texture.Centre);
    }
}