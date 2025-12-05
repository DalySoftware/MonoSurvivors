using System.Collections.Generic;
using Gameplay.Behaviour;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

internal class EntityRenderer(ContentManager content)
{
    private readonly Dictionary<string, Texture2D> _textureCache = new();

    internal void Draw(SpriteBatch spriteBatch, IHasPosition entity)
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