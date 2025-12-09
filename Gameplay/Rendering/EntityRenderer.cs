using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using Gameplay.Entities;
using Gameplay.Rendering.Effects;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class EntityRenderer(ContentManager content, SpriteBatch spriteBatch, ChaseCamera camera, EffectManager effectManager)
{
    private readonly Dictionary<string, Texture2D> _textureCache = [];

    private readonly Effect _grayscaleEffect = content.Load<Effect>(Paths.ShaderEffects.Greyscale);

    public void Draw(IEnumerable<IEntity> entities)
    {
        var visibleBounds = camera.VisibleWorldBounds;
        var effectsLookup = entities
            .OfType<IVisual>()
            .Where(e => IsVisible(e, visibleBounds))
            .ToDictionary(
                e => e,
                e => effectManager.GetEffects(e).ToList());


        spriteBatch.Begin(transformMatrix: camera.Transform);
        foreach (var (entity, _) in effectsLookup.Where(pair => pair.Value.Count == 0))
            Draw(entity);
        spriteBatch.End();

        // Todo - This is quite inefficient. We're individually flushing entities with effects
        foreach (var (entity, effects) in effectsLookup)
            foreach (var effect in effects)
                DrawWithEffect(entity, effect);
    }

    private void Draw(IVisual visual)
    {
        var texture = GetTexture(visual.TexturePath);
        spriteBatch.Draw(texture, visual.Position, origin: texture.Centre);
    }

    private void DrawWithEffect(IVisual visual, VisualEffect effect)
    {
        var texture = GetTexture(visual.TexturePath);
        switch (effect)
        {
            case GreyscaleEffect:
                spriteBatch.Begin(transformMatrix: camera.Transform, effect: _grayscaleEffect);
                spriteBatch.Draw(texture, visual.Position, origin: texture.Centre);
                spriteBatch.End();
                break;
        }
    }

    private Texture2D GetTexture(string path)
    {
        if (_textureCache.TryGetValue(path, out var cached))
            return cached;

        var texture = content.Load<Texture2D>(path);
        _textureCache[path] = texture;
        return texture;
    }

    private static bool IsVisible(IVisual visual, Rectangle visibleBounds)
    {
        const int margin = 300; // Include entities a bit off-screen
        var expandedBounds = new Rectangle(
            visibleBounds.X - margin,
            visibleBounds.Y - margin,
            visibleBounds.Width + margin * 2,
            visibleBounds.Height + margin * 2);

        return expandedBounds.Contains(visual.Position);
    }
}