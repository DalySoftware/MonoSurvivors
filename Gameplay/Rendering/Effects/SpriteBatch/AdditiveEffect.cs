using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering.Effects.SpriteBatch;

public record AdditiveEffect : SpriteBatchEffect
{
    public static BlendState PremultipliedBlend { get; } = new()
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
    };
}