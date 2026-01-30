using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering.Effects;

public sealed class DeathGlitch
{
    private const float Strength = 1f;
    private readonly ConcurrentQueue<SpawnRequest> _pending = new();
    private readonly List<Item> _active = new(64);

    public static TimeSpan Duration { get; } = TimeSpan.FromMilliseconds(200);

    public void Spawn(Texture2D texture, Rectangle? sourceRect, Vector2 position, Vector2 origin, float rotation,
        Vector2 scale, float seed) =>
        _pending.Enqueue(new SpawnRequest(texture, sourceRect, position, origin, rotation, scale, seed));

    public void Update(GameTime gameTime)
    {
        // Drain spawn requests on the main thread
        while (_pending.TryDequeue(out var req))
            _active.Add(new Item(req));

        var dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        for (var i = _active.Count - 1; i >= 0; i--)
        {
            var it = _active[i];
            it.ElapsedMs += dt;

            if (it.ElapsedMs >= Duration.TotalMilliseconds)
                _active.RemoveAt(i);
            else
                _active[i] = it;
        }
    }

    public void Draw(
        Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
        Effect deathGlitchEffect,
        Matrix cameraMatrix,
        GameTime gameTime)
    {
        if (_active.Count == 0) return;

        spriteBatch.Begin(
            SpriteSortMode.Immediate,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            deathGlitchEffect,
            cameraMatrix);

        var time = (float)gameTime.TotalGameTime.TotalSeconds;

        // Only set SourceTexel when texture changes
        Texture2D? lastTexture = null;

        for (var i = 0; i < _active.Count; i++)
        {
            var it = _active[i];
            var progress = MathHelper.Clamp(it.ElapsedMs / (float)Duration.TotalMilliseconds, 0f, 1f);

            var texture = it.Texture;
            if (!ReferenceEquals(texture, lastTexture))
            {
                deathGlitchEffect.Parameters["SourceTexel"]
                    ?.SetValue(new Vector2(1f / texture.Width, 1f / texture.Height));
                lastTexture = texture;
            }

            deathGlitchEffect.Parameters["Time"]?.SetValue(time);
            deathGlitchEffect.Parameters["Progress"]?.SetValue(progress);
            deathGlitchEffect.Parameters["Strength"]?.SetValue(Strength);
            deathGlitchEffect.Parameters["Seed"]?.SetValue(it.Seed);

            spriteBatch.Draw(texture, it.Position, sourceRectangle: it.SourceRect, rotation: it.Rotation,
                origin: it.Origin,
                scale: it.Scale);
        }

        spriteBatch.End();
    }

    private readonly record struct SpawnRequest(
        Texture2D Texture,
        Rectangle? SourceRect,
        Vector2 Position,
        Vector2 Origin,
        float Rotation,
        Vector2 Scale,
        float Seed);

    private struct Item(SpawnRequest req)
    {
        public readonly Texture2D Texture = req.Texture;
        public readonly Rectangle? SourceRect = req.SourceRect;
        public readonly Vector2 Position = req.Position;
        public readonly Vector2 Origin = req.Origin;
        public readonly float Rotation = req.Rotation;
        public readonly Vector2 Scale = req.Scale;
        public float ElapsedMs;
        public readonly float Seed = req.Seed;
    }
}

public interface IRequestDeathGlitch
{
    void EnqueueDeathGlitch(IVisual visual);
}