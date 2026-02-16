using System;
using System.Collections.Generic;
using ContentLibrary;
using Gameplay.Entities.Enemies.Types;
using Gameplay.Entities.Pooling;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Effects;

internal sealed class ScorcherGlow(Scorcher scorcher, ISpawnEntity spawner, EmberPool emberPool, ContentManager content)
    : IEntity
{
    private float _time;
    private float _spawnAcc;

    // Used to seed ember rng in a pseudo random way
    private readonly float _phase = scorcher.Position.X * 0.013f + scorcher.Position.Y * 0.017f;

    private readonly List<Vector2> _fireOffsets =
        BuildOffsetsFromMask(content.Load<Texture2D>(Paths.Images.ScorcherEmberMask));

    public bool MarkedForDeletion { get; private set; }
    public Vector2 Position => scorcher.Position;

    public void Update(GameTime gameTime)
    {
        if (scorcher.MarkedForDeletion)
        {
            MarkedForDeletion = true;
            return;
        }

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _time += dt;

        _spawnAcc += dt;
        const float interval = 0.1f;

        while (_spawnAcc >= interval)
        {
            _spawnAcc -= interval;
            TrySpawnEmber();
        }
    }

    private static List<Vector2> BuildOffsetsFromMask(Texture2D emberMask)
    {
        var w = emberMask.Width;
        var h = emberMask.Height;

        var data = new Color[w * h];
        emberMask.GetData(data);

        var origin = new Vector2(w * 0.5f, h * 0.5f);

        var offsets = new List<Vector2>(256);

        for (var y = 0; y < h; y++)
        for (var x = 0; x < w; x++)
        {
            var a = data[y * w + x].A;
            if (a == 0) continue;

            var hash = (byte)((x * 17 + y * 31) & 255);
            if (hash >= a) continue;

            if (((x ^ y) & 1) != 0) continue;

            offsets.Add(new Vector2(x + 0.5f, y + 0.5f) - origin);
        }

        return offsets;
    }

    private void TrySpawnEmber()
    {
        if (_fireOffsets.Count == 0)
            return;

        // deterministic-ish but varied using _phase
        var i = (int)((_time + _phase) * 97f) % _fireOffsets.Count;
        if (i < 0) i += _fireOffsets.Count;

        var localOffset = _fireOffsets[i];
        var position = Position + localOffset;

        // upward with jitter
        var vx = 20f * MathF.Sin(_time * 13.7f + i);
        var vy = -40f - 25f * (0.5f + 0.5f * MathF.Sin(_time * 9.1f + i * 0.3f));
        var velocity = new Vector2(vx, vy);

        spawner.Spawn(emberPool.Get(position, velocity));
    }
}

internal sealed class Ember(EmberPool pool) : IEntity, IPrimitiveVisual, IPoolableEntity
{
    private readonly static TimeSpan Lifetime = TimeSpan.FromSeconds(0.45);

    private Vector2 _vel;
    private TimeSpan _t;

    public bool MarkedForDeletion { get; private set; }
    public Vector2 Position { get; private set; }
    public float Layer => Layers.Projectiles;

    public Ember Reinitialize(Vector2 position, Vector2 velocity)
    {
        Position = position;
        _vel = velocity;
        _t = TimeSpan.Zero;
        MarkedForDeletion = false;
        return this;
    }

    public void Update(GameTime gameTime)
    {
        _t += gameTime.ElapsedGameTime;
        if (_t >= Lifetime)
        {
            MarkedForDeletion = true;
            return;
        }

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _vel *= MathF.Pow(0.10f, dt);
        _vel += new Vector2(0f, -18f) * dt;

        Position += _vel * dt;
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer primitiveRenderer)
    {
        var p = (float)(_t / Lifetime);
        var alpha = 1f - p;
        alpha *= alpha;

        var radius = Math.Max(2, (int)MathF.Round(8f * (1f - p)));
        var c = Color.Lerp(ColorPalette.Yellow, ColorPalette.Red, p) * (1f * alpha);

        primitiveRenderer.DrawSoftCircle(spriteBatch, Position, radius, c, layerDepth: Layer);
    }

    public void OnDespawned() => pool.Return(this);
}

public sealed class EmberPool
{
    private readonly Stack<Ember> _pool = new();

    internal Ember Get(Vector2 position, Vector2 velocity)
    {
        if (!_pool.TryPop(out var ember))
            ember = new Ember(this);

        return ember.Reinitialize(position, velocity);
    }

    internal void Return(Ember ember) => _pool.Push(ember);
}