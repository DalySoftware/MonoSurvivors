using Gameplay.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Effects;

public class DamageAuraEffect(PlayerCharacter owner, GraphicsDevice graphicsDevice) : IEntity, IGenericVisual
{
    private Texture2D? _circleTexture;

    public float Range
    {
        set
        {
            field = value;
            _circleTexture = PrimitiveRenderer.CreateSoftCircle(graphicsDevice, (int)field,
                PrimitiveRenderer.SoftCircleMaskType.InsideTransparent, 0.8f);
        }
    }

    public void Update(GameTime gameTime) { } // Range setter handles updates already

    public float Layer => Layers.Pickups - 0.02f;
    public Vector2 Position => owner.Position;

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_circleTexture is not null)
            spriteBatch.Draw(
                _circleTexture,
                Position,
                Color.Plum * 0.25f,
                origin: _circleTexture.Centre,
                layerDepth: Layer);
    }
}