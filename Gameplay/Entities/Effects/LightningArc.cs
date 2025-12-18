using System;
using Gameplay.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Effects;

internal sealed class LightningArc(Vector2 from, Vector2 to) : IEntity, IPrimitiveVisual
{
    private TimeSpan _lifetime = TimeSpan.FromMilliseconds(60);

    public bool MarkedForDeletion { get; private set; }

    public void Update(GameTime gameTime)
    {
        _lifetime -= gameTime.ElapsedGameTime;
        if (_lifetime <= TimeSpan.Zero)
            MarkedForDeletion = true;
    }
    public Vector2 Position => (from + to) * 0.5f;
    public float Layer => Layers.Projectiles;

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer) =>
        renderer.DrawLine(spriteBatch, from, to, Color.Yellow, 10f, Layer);
}