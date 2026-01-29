using System;
using System.Collections.Generic;
using ContentLibrary;
using Gameplay.Entities;
using Gameplay.Entities.Effects;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.Effects.SpriteBatch;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class EntityRenderer(
    ContentManager content,
    SpriteBatch spriteBatch,
    ChaseCamera camera,
    EffectManager effectManager,
    PrimitiveRenderer primitiveRenderer,
    OutlineRenderer outlineRenderer) : IRequestDeathGlitch
{
    // Used to slightly bias lower enemies towards front. Makes render roughly deterministic
    private const float YSortScale = 0.00000000001f;

    private readonly Effect _grayscaleEffect = content.Load<Effect>(Paths.ShaderEffects.Greyscale);
    private readonly Effect _silhouetteEffect = content.Load<Effect>(Paths.ShaderEffects.Silhouette);

    private readonly Dictionary<string, Texture2D> _textureCache = [];

    private readonly Dictionary<SpriteBatchEffect, List<IVisual>> _effectBuckets = new();
    private readonly Stack<List<SpriteBatchEffect>> _effectsListPool = new();
    private readonly List<FlashDraw> _flashDraws = new(128);

    private readonly DeathGlitch _deathGlitch = new();
    private readonly Effect _deathGlitchEffect = content.Load<Effect>(Paths.ShaderEffects.DeathGlitch);

    public void EnqueueDeathGlitch(IVisual visual)
    {
        if (!TryBuildSpriteDraw(visual, out var sprite))
            return;

        // Seed: stable-ish, no need for RNG state elsewhere.
        var seed = sprite.Position.X * 0.013f + sprite.Position.Y * 0.017f;

        _deathGlitch.Spawn(sprite.Texture, sprite.SourceRect, sprite.Position, sprite.Origin, 0f, sprite.Scale, seed);
    }

    private List<SpriteBatchEffect> RentEffectsList() => _effectsListPool.Count > 0
        ? _effectsListPool.Pop()
        : new List<SpriteBatchEffect>(4);

    private void ReturnEffectsList(List<SpriteBatchEffect> list)
    {
        list.Clear();
        _effectsListPool.Push(list);
    }

    public void Update(GameTime gameTime) => _deathGlitch.Update(gameTime);

    /// <summary>
    ///     Draws entities that do NOT require EntityRenderer-managed SpriteBatch.Begin/End.
    ///     (Caller is expected to manage SpriteBatch.Begin/End around this.)
    /// </summary>
    public void Draw(IReadOnlyList<IEntity> entities)
    {
        _effectBuckets.Clear();
        _flashDraws.Clear();

        var visibleBounds = camera.VisibleWorldBounds;

        for (var i = 0; i < entities.Count; i++)
        {
            if (entities[i] is not IVisual visual)
                continue;

            if (!IsVisible(visual, visibleBounds))
                continue;

            var hasSprite = TryBuildSpriteDraw(visual, out var sprite);
            if (hasSprite && visual is IHasHitFlash { FlashIntensity: > 0f } flash)
                _flashDraws.Add(new FlashDraw(sprite, flash.FlashColor * flash.FlashIntensity));

            var list = RentEffectsList();

            foreach (var fx in effectManager.GetEffects(visual))
                list.Add(fx.Effect);

            if (list.Count == 0)
            {
                ReturnEffectsList(list);
                if (hasSprite) DrawSprite(sprite, visual);
                else Draw(visual);

                continue;
            }

            foreach (var fx in list)
            {
                if (!_effectBuckets.TryGetValue(fx, out var visuals))
                    _effectBuckets[fx] = visuals = [];

                visuals.Add(visual);
            }

            ReturnEffectsList(list);
        }
    }

    private bool TryBuildSpriteDraw(IVisual visual, out SpriteDraw draw)
    {
        var scale = (visual as IHasDrawTransform)?.DrawScale ?? Vector2.One;

        switch (visual)
        {
            case ISpriteSheetVisual v:
            {
                var texture = v.SpriteSheet.Texture;
                var src = v.SpriteSheet.GetFrameRectangle(v.CurrentFrame);
                var origin = new Vector2(src.Width * 0.5f, src.Height * 0.5f);
                var layer = v.Layer + v.Position.Y * YSortScale;


                draw = new SpriteDraw(texture, v.Position, src, origin, layer, scale);
                return true;
            }


            case ISpriteVisual v:
            {
                var texture = GetTexture(v.TexturePath);
                var origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
                var layer = v.Layer + v.Position.Y * YSortScale;


                draw = new SpriteDraw(texture, v.Position, null, origin, layer, scale);
                return true;
            }


            default:
                draw = default;
                return false;
        }
    }

    /// <summary>
    ///     Draws the subset of entities/effects that require EntityRenderer to manage SpriteBatch.Begin/End.
    ///     Call this after Draw() and after ending your main SpriteBatch.
    /// </summary>
    public void DrawManagedPasses(GameTime gameTime)
    {
        DrawManagedEffectsPass();
        DrawHitFlashPass();
        DrawDeathGlitchPass(gameTime);
    }

    private void DrawDeathGlitchPass(GameTime gameTime) =>
        _deathGlitch.Draw(spriteBatch, _deathGlitchEffect, camera.Transform, gameTime);

    private void DrawManagedEffectsPass()
    {
        foreach (var (fx, visuals) in _effectBuckets)
        {
            BeginForEffect(fx);
            foreach (var visual in visuals) Draw(visual);
            spriteBatch.End();

            visuals.Clear();
        }
    }

    private void DrawHitFlashPass()
    {
        if (_flashDraws.Count == 0)
            return;

        spriteBatch.Begin(
            transformMatrix: camera.Transform,
            blendState: BlendState.Additive,
            sortMode: SpriteSortMode.FrontToBack,
            effect: _silhouetteEffect);

        foreach (var f in _flashDraws)
            // f.Color already includes intensity
            DrawSprite(f.Sprite, f.Color); // draws original texture, effect turns it into silhouette

        spriteBatch.End();
        _flashDraws.Clear();
    }


    private void BeginForEffect(SpriteBatchEffect fx)
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
        if (TryBuildSpriteDraw(visual, out var sprite))
        {
            DrawSprite(sprite, visual);
            return;
        }

        switch (visual)
        {
            case IPrimitiveVisual primitiveVisual:
                primitiveVisual.Draw(spriteBatch, primitiveRenderer);
                break;
            case IGenericVisual genericVisual:
                genericVisual.Draw(spriteBatch);
                break;
        }
    }

    private void DrawSprite(SpriteDraw sprite, IVisual visual)
    {
        Color? outline = visual is ISpriteSheetVisual { OutlineColor: { } color } ? color : null;
        DrawSprite(sprite, outlineColor: outline);
    }

    private void DrawSprite(SpriteDraw sprite, Color? color = null, Color? outlineColor = null)
    {
        color ??= ColorPalette.White;
        if (outlineColor is { } outline)
            // SourceRect must exist for sheets
            outlineRenderer.DrawOutline(
                spriteBatch,
                sprite.Texture,
                sprite.Position,
                sprite.SourceRect,
                sprite.Origin,
                sprite.Layer - 0.001f,
                outline,
                sprite.Scale);

        spriteBatch.Draw(sprite.Texture, sprite.Position,
            sourceRectangle: sprite.SourceRect, origin: sprite.Origin, scale: sprite.Scale, layerDepth: sprite.Layer,
            color: color);
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

    private readonly record struct SpriteDraw(
        Texture2D Texture,
        Vector2 Position,
        Rectangle? SourceRect,
        Vector2 Origin,
        float Layer,
        Vector2 Scale);

    private readonly record struct FlashDraw(SpriteDraw Sprite, Color Color);
}