using System;
using ContentLibrary;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class PrimitiveRenderer(ContentManager content, GraphicsDevice graphicsDevice)
{
    private readonly Texture2D _pixelTexture = CreatePixelTexture(graphicsDevice);
    private readonly Texture2D _triangleTexture = content.Load<Texture2D>(Paths.Images.PanelTriangle);

    private static Texture2D CreatePixelTexture(GraphicsDevice graphicsDevice)
    {
        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData([Color.White]);
        return texture;
    }

    public void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness,
        float layerDepth = 0f)
    {
        var distance = Vector2.Distance(start, end);
        var angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

        spriteBatch.Draw(_pixelTexture, start, null, color, angle, Vector2.Zero,
            new Vector2(distance, thickness), SpriteEffects.None, layerDepth);
    }

    public void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color, float layerDepth = 0f) =>
        spriteBatch.Draw(_pixelTexture, rect, color, layerDepth: layerDepth);

    /// <summary>
    ///     Default texture is 28x28. Hypotenuse from NW-SE. SW section transparent, NE section white.
    /// </summary>
    public void DrawTriangle(
        SpriteBatch spriteBatch,
        Vector2 position,
        Color color,
        float rotation = 0f,
        float layerDepth = 0f) => spriteBatch.Draw(
        _triangleTexture,
        position,
        color,
        rotation: rotation,
        layerDepth: layerDepth
    );
}