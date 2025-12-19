using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using Gameplay.Entities;
using Gameplay.Rendering.Effects;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class EntityRenderer(
    ContentManager content,
    SpriteBatch spriteBatch,
    ChaseCamera camera,
    EffectManager effectManager,
    PrimitiveRenderer primitiveRenderer)
{
    private readonly Effect _grayscaleEffect = content.Load<Effect>(Paths.ShaderEffects.Greyscale);
    private readonly Dictionary<string, Texture2D> _textureCache = [];

    public void Draw(IEnumerable<IEntity> entities)
    {
        var visibleBounds = camera.VisibleWorldBounds;
        var effectsLookup = entities
            .OfType<IVisual>()
            .Where(e => IsVisible(e, visibleBounds))
            .ToDictionary(
                e => e,
                e => effectManager.GetEffects(e).ToList());

        spriteBatch.Begin(transformMatrix: camera.Transform, sortMode: SpriteSortMode.FrontToBack);
        foreach (var (entity, _) in effectsLookup.Where(pair => pair.Value.Count == 0))
            Draw(entity);
        spriteBatch.End();

        foreach (var (entity, effects) in effectsLookup)
        foreach (var effect in effects)
            DrawWithEffect(entity, effect);
    }

    private void Draw(IVisual visual)
    {
        switch (visual)
        {
            case ISpriteSheetVisual spriteSheetVisual:
                DrawFromSpriteSheet(spriteSheetVisual);
                break;
            case ISpriteVisual simpleVisual:
                DrawSimpleSprite(simpleVisual);
                break;
            case IPrimitiveVisual primitiveVisual:
                primitiveVisual.Draw(spriteBatch, primitiveRenderer);
                break;
        }
    }

    private void DrawFromSpriteSheet(ISpriteSheetVisual visual)
    {
        var texture = visual.SpriteSheet.Texture(content);
        var sourceRect = visual.SpriteSheet.GetFrameRectangle(visual.CurrentFrame);
        var origin = new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f);
        spriteBatch.Draw(texture, visual.Position, sourceRectangle: sourceRect, origin: origin,
            layerDepth: visual.Layer);
    }

    private void DrawSimpleSprite(ISpriteVisual visual)
    {
        var texture = GetTexture(visual.TexturePath);
        var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        spriteBatch.Draw(texture, visual.Position, origin: origin, layerDepth: visual.Layer);
    }

    private void DrawWithEffect(IVisual visual, VisualEffect effect)
    {
        switch (effect)
        {
            case GreyscaleEffect:
                spriteBatch.Begin(transformMatrix: camera.Transform, effect: _grayscaleEffect,
                    sortMode: SpriteSortMode.FrontToBack);
                Draw(visual);
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
        const int margin = 600; // Include entities a decent amount off screen
        var expandedBounds = new Rectangle(
            visibleBounds.X - margin,
            visibleBounds.Y - margin,
            visibleBounds.Width + margin * 2,
            visibleBounds.Height + margin * 2);

        return expandedBounds.Contains(visual.Position);
    }
}