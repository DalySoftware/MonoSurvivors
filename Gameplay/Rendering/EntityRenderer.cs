using System;
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

    private readonly Dictionary<VisualEffect, List<IVisual>> _effectBuckets = new();
    private readonly Stack<List<VisualEffect>> _effectsListPool = new();

    private List<VisualEffect> RentEffectsList() => _effectsListPool.Count > 0
        ? _effectsListPool.Pop()
        : new List<VisualEffect>(4);

    private void ReturnEffectsList(List<VisualEffect> list)
    {
        list.Clear();
        _effectsListPool.Push(list);
    }

    /// <summary>
    ///     Draws entities that do NOT require EntityRenderer-managed SpriteBatch.Begin/End.
    ///     (Caller is expected to manage SpriteBatch.Begin/End around this.)
    /// </summary>
    public void Draw(IReadOnlyList<IEntity> entities)
    {
        _effectBuckets.Clear();

        var visibleBounds = camera.VisibleWorldBounds;

        for (var i = 0; i < entities.Count; i++)
        {
            if (entities[i] is not IVisual e)
                continue;

            if (!IsVisible(e, visibleBounds))
                continue;

            var list = RentEffectsList();

            foreach (var fx in effectManager.GetEffects(e))
                list.Add(fx.Effect);

            if (list.Count == 0)
            {
                ReturnEffectsList(list);
                Draw(e);
                continue;
            }

            foreach (var fx in list)
            {
                if (!_effectBuckets.TryGetValue(fx, out var visuals))
                    _effectBuckets[fx] = visuals = [];

                visuals.Add(e);
            }

            ReturnEffectsList(list);
        }
    }

    /// <summary>
    ///     Draws the subset of entities/effects that require EntityRenderer to manage SpriteBatch.Begin/End.
    ///     Call this after Draw() and after ending your main SpriteBatch.
    /// </summary>
    public void DrawManagedEffects()
    {
        foreach (var (fx, visuals) in _effectBuckets)
        {
            BeginForEffect(fx);
            foreach (var visual in visuals) Draw(visual);
            spriteBatch.End();

            visuals.Clear();
        }
    }

    private void BeginForEffect(VisualEffect fx)
    {
        switch (fx)
        {
            case GreyscaleEffect:
                spriteBatch.Begin(
                    transformMatrix: camera.Transform,
                    effect: _grayscaleEffect,
                    sortMode: SpriteSortMode.FrontToBack);
                return;

            default:
                throw new InvalidOperationException($"Unhandled managed effect: {fx.GetType().Name}");
        }
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
        var scale = GetScale(visual);

        if (visual.OutlineColor is { } outlineColor)
            outlineRenderer.DrawOutline(spriteBatch, texture, visual.Position, sourceRect, origin,
                layer - 0.001f, outlineColor, scale);

        spriteBatch.Draw(texture, visual.Position, sourceRectangle: sourceRect, origin: origin,
            layerDepth: layer, scale: scale);
    }

    private void DrawSimpleSprite(ISpriteVisual visual)
    {
        var texture = GetTexture(visual.TexturePath);
        var origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

        var layer = visual.Layer + visual.Position.Y * YSortScale;
        spriteBatch.Draw(texture, visual.Position, origin: origin, layerDepth: layer);
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

    private Vector2 GetScale(IVisual visual)
    {
        if (visual is IHasDrawTransform transform) return transform.DrawScale;

        return Vector2.One;
    }
}