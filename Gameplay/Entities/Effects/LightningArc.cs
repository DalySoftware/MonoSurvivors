using System;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Effects;

internal sealed class LightningArc(Vector2 from, Vector2 to) : IEntity, IPrimitiveVisual
{
    private readonly static TimeSpan MaxLifetime = TimeSpan.FromMilliseconds(180);
    private readonly Vector2 _jitter = new((Random.Shared.NextSingle() - 0.5f) * 12f,
        (Random.Shared.NextSingle() - 0.5f) * 12f);
    private TimeSpan _lifetime = MaxLifetime;

    public bool MarkedForDeletion { get; private set; }

    public void Update(GameTime gameTime)
    {
        _lifetime -= gameTime.ElapsedGameTime;
        if (_lifetime <= TimeSpan.Zero)
            MarkedForDeletion = true;
    }

    public Vector2 Position => (from + to) * 0.5f;
    public float Layer => Layers.Projectiles;

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        var t = (float)(_lifetime.TotalMilliseconds / MaxLifetime.TotalMilliseconds);
        var thickness = MathHelper.Lerp(12f, 2f, 1f - t);
        var baseColor = ColorPalette.Royal;

        renderer.DrawLine(spriteBatch, from + _jitter, to + _jitter,
            baseColor * 0.3f * t, thickness * 2f, Layer - 0.01f);
        renderer.DrawLine(spriteBatch, from, to, baseColor.ShiftLightness(0.5f) * t, thickness, Layer);
    }
}