using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop;

public static class SpriteBatchExtensions
{
    extension(SpriteBatch spriteBatch)
    {
        public void Draw(Texture2D texture, Vector2 position, Color? color = null, Rectangle? sourceRectangle = null,
            float rotation = 0f, Vector2? origin = null, Vector2? scale = null, SpriteEffects? effects = null, float layerDepth = 0f) =>
            spriteBatch.Draw(texture, position, sourceRectangle, color ?? Color.White, rotation, origin ?? Vector2.Zero, scale ?? Vector2.One, effects ?? SpriteEffects.None, layerDepth);
    }
}