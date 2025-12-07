using System.Collections.Generic;
using System.Linq;
using Gameplay.Behaviour;
using Gameplay.Entities;
using Gameplay.Rendering.Effects;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class EntityRenderer(ContentManager content, SpriteBatch spriteBatch, ChaseCamera camera, EffectManager effectManager)
{
    private readonly Dictionary<string, Texture2D> _textureCache = new();

    public void Draw(IEnumerable<IEntity> entities)
    {
        spriteBatch.Begin(transformMatrix: camera.Transform);
        foreach (var entity in entities.OfType<IHasPosition>()) Draw(entity);
        spriteBatch.End();
    }

    private void Draw(IHasPosition entity)
    {
        if (entity is not IVisual visual) return;

        var texture = GetTexture(visual.TexturePath);
        var effects = effectManager.GetEffects(entity);
        var color = GetEffectiveColor(effects);
        spriteBatch.Draw(texture, entity.Position, color: color, origin: texture.Centre);
    }

    private static Color GetEffectiveColor(IReadOnlyList<VisualEffect> effects) =>
        effects.FirstOrDefault()?.ComputeColor() ?? Color.White;

    private Texture2D GetTexture(string path)
    {
        if (_textureCache.TryGetValue(path, out var cached))
            return cached;

        var texture = content.Load<Texture2D>(path);
        _textureCache[path] = texture;
        return texture;
    }
}