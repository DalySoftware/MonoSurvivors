using System;
using System.Collections.Generic;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Effects;

public class DamageAuraEffect(PlayerCharacter owner, GraphicsDevice graphicsDevice) : IEntity, IGenericVisual
{
    private Texture2D? _circleTexture;

    private readonly List<Ripple> _activeRipples = [];

    public float Range
    {
        get;
        set
        {
            field = value;
            _circleTexture = PrimitiveRenderer.CreateSoftCircle(graphicsDevice, (int)field,
                PrimitiveRenderer.SoftCircleMaskType.InsideTransparent, 0.8f);
        }
    }

    public void Update(GameTime gameTime)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        for (var i = _activeRipples.Count - 1; i >= 0; i--)
        {
            var ripple = _activeRipples[i];
            var speed = (ripple.MaxRadius - ripple.CurrentRadius) * 2f; // expansion speed
            var newRadius = ripple.CurrentRadius + speed * delta;

            // progress relative to spawn radius
            var progress = (newRadius - ripple.CurrentRadius) / (ripple.MaxRadius - ripple.CurrentRadius);
            progress = MathF.Min(progress, 1f);

            var newAlpha = ripple.Alpha * (1f - progress); // fade from initial alpha to 0

            if (newAlpha <= 0f)
            {
                _activeRipples.RemoveAt(i);
                continue;
            }

            _activeRipples[i] = ripple with
            {
                CurrentRadius = newRadius, Alpha = newAlpha, Position = owner.Position,
            };
        }
    }

    public float Layer => Layers.Pickups - 0.02f;
    public Vector2 Position => owner.Position;

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_circleTexture is not null)
            spriteBatch.Draw(
                _circleTexture,
                Position,
                ColorPalette.Ice * 0.4f,
                origin: _circleTexture.Centre,
                layerDepth: Layer);

        // draw ripples
        foreach (var r in _activeRipples)
        {
            var scale = r.CurrentRadius / Range; // scale based on current radius
            spriteBatch.Draw(
                r.Texture,
                r.Position,
                ColorPalette.Ice * r.Alpha,
                origin: r.Texture.Centre,
                scale: new Vector2(scale, scale),
                layerDepth: Layer + 0.001f); // slightly above base aura
        }
    }

    public void SpawnRipple()
    {
        if (_circleTexture is null) return;

        _activeRipples.Add(new Ripple(_circleTexture, owner.Position, Range * 0.6f, Range * 1.1f, 0.8f));
        // 2nd starts further in so moves faster
        _activeRipples.Add(new Ripple(_circleTexture, owner.Position, Range * 0.4f, Range * 1.1f, 1f));
    }

    private record Ripple(Texture2D Texture, Vector2 Position, float CurrentRadius, float MaxRadius, float Alpha);
}