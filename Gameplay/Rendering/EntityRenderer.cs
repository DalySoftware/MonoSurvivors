using System.Collections.Generic;
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
    PrimitiveRenderer primitiveRenderer,
    OutlineRenderer outlineRenderer)
{
    // Used to slightly bias lower enemies towards front. Makes render roughly deterministic
    private const float YSortScale = 0.00000000001f;

    private readonly Effect _grayscaleEffect = content.Load<Effect>(Paths.ShaderEffects.Greyscale);
    private readonly Dictionary<string, Texture2D> _textureCache = [];

    private readonly Dictionary<IVisual, List<VisualEffect>> _effectsLookup = new();
    private readonly Stack<List<VisualEffect>> _effectsListPool = new();

    private List<VisualEffect> RentEffectsList() => _effectsListPool.Count > 0
        ? _effectsListPool.Pop()
        : new List<VisualEffect>(4);

    private void ReturnEffectsList(List<VisualEffect> list)
    {
        list.Clear();
        _effectsListPool.Push(list);
    }

    private void FlushEffectsLookup()
    {
        foreach (var list in _effectsLookup.Values)
            ReturnEffectsList(list);

        _effectsLookup.Clear();
    }

    // Effects that require this renderer to manage SpriteBatch.Begin/End internally
    private static bool RequiresManagedSpriteBatch(VisualEffect effect) => effect is GreyscaleEffect;

    /// <summary>
    ///     Draws entities that do NOT require EntityRenderer-managed SpriteBatch.Begin/End.
    ///     (Caller is expected to manage SpriteBatch.Begin/End around this.)
    /// </summary>
    public void Draw(IReadOnlyList<IEntity> entities)
    {
        // In case the caller didn't call DrawManagedEffects() last frame/pass
        FlushEffectsLookup();

        var visibleBounds = camera.VisibleWorldBounds;

        for (var i = 0; i < entities.Count; i++)
        {
            if (entities[i] is not IVisual e)
                continue;

            if (!IsVisible(e, visibleBounds))
                continue;

            var list = RentEffectsList();

            foreach (var fx in effectManager.GetEffects(e))
                list.Add(fx);

            _effectsLookup.Add(e, list);
        }

        foreach (var (entity, effects) in _effectsLookup)
        {
            // If it has any effects that require managed SpriteBatch, skip it here.
            // It will be drawn in DrawManagedEffects().
            if (HasManagedEffect(effects))
                continue;

            Draw(entity);
        }
    }

    private static bool HasManagedEffect(List<VisualEffect> effects)
    {
        for (var i = 0; i < effects.Count; i++)
            if (RequiresManagedSpriteBatch(effects[i]))
                return true;

        return false;
    }


    /// <summary>
    ///     Draws the subset of entities/effects that require EntityRenderer to manage SpriteBatch.Begin/End.
    ///     Call this after Draw() (typically after ending your main SpriteBatch).
    /// </summary>
    public void DrawManagedEffects()
    {
        foreach (var (entity, effects) in _effectsLookup)
        foreach (var effect in effects)
        {
            if (!RequiresManagedSpriteBatch(effect))
                continue;

            DrawWithManagedEffect(entity, effect);
        }

        // Return pooled lists
        FlushEffectsLookup();
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
            case IGenericVisual genericVisual:
                genericVisual.Draw(spriteBatch);
                break;
        }
    }

    private void DrawFromSpriteSheet(ISpriteSheetVisual visual)
    {
        var texture = visual.SpriteSheet.Texture;
        var sourceRect = visual.SpriteSheet.GetFrameRectangle(visual.CurrentFrame);
        var origin = new Vector2(sourceRect.Width * 0.5f, sourceRect.Height * 0.5f);

        var layer = visual.Layer + visual.Position.Y * YSortScale;

        if (visual.OutlineColor is { } outlineColor)
            outlineRenderer.DrawOutline(spriteBatch, texture, visual.Position, sourceRect, origin,
                layer - 0.001f, outlineColor);

        spriteBatch.Draw(texture, visual.Position, sourceRectangle: sourceRect, origin: origin,
            layerDepth: layer);
    }

    private void DrawSimpleSprite(ISpriteVisual visual)
    {
        var texture = GetTexture(visual.TexturePath);
        var origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

        var layer = visual.Layer + visual.Position.Y * YSortScale;
        spriteBatch.Draw(texture, visual.Position, origin: origin, layerDepth: layer);
    }

    private void DrawWithManagedEffect(IVisual visual, VisualEffect effect)
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