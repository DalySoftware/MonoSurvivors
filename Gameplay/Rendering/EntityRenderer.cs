using System.Collections.Generic;
using System.Linq;
using Gameplay.Behaviour;
using Gameplay.Entities;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class EntityRenderer(ContentManager content, SpriteBatch spriteBatch)
{
    private readonly Dictionary<string, Texture2D> _textureCache = new();

    public void Draw(IEnumerable<IEntity> entities)
    {
        spriteBatch.Begin();
        foreach (var entity in entities.OfType<IHasPosition>()) Draw(entity);
        spriteBatch.End();
    }

    private void Draw(IHasPosition entity)
    {
        if (entity is not IVisual visual) return;

        var texture = GetTexture(visual.TexturePath);
        spriteBatch.Draw(texture, entity.Position, origin: texture.Centre);
    }

    private Texture2D GetTexture(string path)
    {
        if (_textureCache.TryGetValue(path, out var cached))
            return cached;

        var texture = content.Load<Texture2D>(path);
        _textureCache[path] = texture;
        return texture;
    }
}