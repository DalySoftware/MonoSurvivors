using System;
using System.Collections.Generic;
using Gameplay.Entities.Pooling;
using Gameplay.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Effects;

internal sealed class BloomPuff(BloomPuffPool pool) : IEntity, IPrimitiveVisual, IPoolableEntity
{
    private float _progress; // 0 - 1
    private TimeSpan _duration;
    private int _startRadius;
    private int _endRadius;
    private Color _color;

    public bool MarkedForDeletion { get; private set; }
    public Vector2 Position { get; private set; }
    public float Layer => Layers.Projectiles;

    public BloomPuff Reinitialize(Vector2 position, Color color, int startRadius, int endRadius, TimeSpan duration)
    {
        Position = position;
        _color = color;

        _startRadius = startRadius;
        _endRadius = endRadius;

        _duration = duration;
        _progress = 0f;
        MarkedForDeletion = false;

        return this;
    }

    public void Update(GameTime gameTime)
    {
        _progress += (float)(gameTime.ElapsedGameTime / _duration);
        if (_progress >= 1f) MarkedForDeletion = true;
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer primitiveRenderer)
    {
        var e = 1f - (1f - _progress) * (1f - _progress);

        var r = MathHelper.Lerp(_startRadius, _endRadius, e);

        // Snap to a small set of radii to prevent loads of textures
        const int radiusStep = 4;
        var radius = Math.Max(radiusStep, (int)(MathF.Round(r / radiusStep) * radiusStep));

        var alpha = 1f - _progress;
        alpha *= alpha;

        primitiveRenderer.DrawSoftCircle(spriteBatch, Position, radius, _color * alpha, layerDepth: Layer);
    }

    public void OnDespawned() => pool.Return(this);
}

public class BloomPuffPool
{
    private readonly Stack<BloomPuff> _pool = new();

    internal BloomPuff Get(Vector2 position, Color color, int startRadius, int endRadius, TimeSpan duration)
    {
        if (!_pool.TryPop(out var puff)) puff = new BloomPuff(this);
        return puff.Reinitialize(position, color, startRadius, endRadius, duration);
    }

    internal void Return(BloomPuff puff) => _pool.Push(puff);
}