using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public static class RenderingHelpers
{
    extension(SpriteBatch spriteBatch)
    {
        public void Draw(Texture2D texture,
            Vector2 position,
            Color? color = null,
            Rectangle? sourceRectangle = null,
            float rotation = 0f,
            Vector2? origin = null,
            Vector2? scale = null,
            SpriteEffects? effects = null,
            float layerDepth = 0f) =>
            spriteBatch.Draw(texture,
                position,
                sourceRectangle,
                color ?? Color.White,
                rotation,
                origin ?? Vector2.Zero,
                scale ?? Vector2.One,
                effects ?? SpriteEffects.None,
                layerDepth);

        public void Draw(Texture2D texture,
            Rectangle destinationRectangle,
            Color? color = null,
            Rectangle? sourceRectangle = null,
            float rotation = 0f,
            Vector2? origin = null,
            SpriteEffects? effects = null,
            float layerDepth = 0f) =>
            spriteBatch.Draw(texture,
                destinationRectangle,
                sourceRectangle,
                color ?? Color.White,
                rotation,
                origin ?? Vector2.Zero,
                effects ?? SpriteEffects.None,
                layerDepth);

        // Texture2D texture,
        //     Rectangle destinationRectangle,
        // Rectangle? sourceRectangle,
        //     Color color,
        // float rotation,
        //     Vector2 origin,
        // SpriteEffects effects,
        // float layerDepth)

        public void DrawString(SpriteFont font,
            string text,
            Vector2 position,
            Color? color = null,
            float rotation = 0f,
            Vector2? origin = null,
            Vector2? scale = null,
            SpriteEffects? effects = null,
            float layerDepth = 0f) =>
            spriteBatch.DrawString(
                font,
                text,
                position,
                color ?? Color.White,
                rotation,
                origin ?? Vector2.Zero,
                scale ?? Vector2.One,
                effects ?? SpriteEffects.None,
                layerDepth);
    }

    extension(Texture2D texture)
    {
        public Vector2 Centre => new Vector2(texture.Width, texture.Height) * 0.5f;
    }

    extension(GameWindow window)
    {
        public Vector2 Centre => new Vector2(window.ClientBounds.Width, window.ClientBounds.Height) * 0.5f;
    }
}