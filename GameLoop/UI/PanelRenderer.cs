using System;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

public sealed class PanelRenderer(ContentManager content, PrimitiveRenderer primitiveRenderer)
{
    private readonly NineSliceFrame _frame = new(content);

    public static Vector2 GetExteriorSize(Vector2 interiorSize)
    {
        // Arbitrary position
        var rect = NineSliceFrame.GetExteriorRect(new Vector2(0, 0), interiorSize);
        return new Vector2(rect.Width, rect.Height);
    }

    public static Vector2 GetCenter(Vector2 position, Vector2 interiorSize)
    {
        var size = GetExteriorSize(interiorSize);
        return position + size * 0.5f;
    }

    public void Draw(
        SpriteBatch spriteBatch,
        Vector2 topLeft,
        Vector2 interiorSize,
        Color frameColor,
        Color interiorColor,
        float layerDepth = 0f)
    {
        DrawInterior(spriteBatch, topLeft, interiorSize, interiorColor, layerDepth);

        DrawInnerCorners(spriteBatch, topLeft, interiorSize, interiorColor, layerDepth);
        DrawEdgeInteriors(spriteBatch, topLeft, interiorSize, interiorColor, layerDepth);

        DrawFrame(spriteBatch, topLeft, interiorSize, frameColor, layerDepth);
    }
    private void DrawInterior(SpriteBatch spriteBatch, Vector2 topLeft, Vector2 interiorSize, Color interiorColor,
        float layerDepth)
    {
        var interiorRect = NineSliceFrame.GetInteriorRect(topLeft, interiorSize);
        primitiveRenderer.DrawRectangle(spriteBatch, interiorRect, interiorColor, layerDepth);
    }

    private void DrawInnerCorners(SpriteBatch spriteBatch, Vector2 topLeft, Vector2 interiorSize, Color interiorColor,
        float layerDepth)
    {
        var farCornerOffset = interiorSize + Vector2.One * NineSliceFrame.CornerSize * 2f;
        var corners = new[]
        {
            (pos: topLeft, rotation: 0f), // TopLeft, no rotation
            (pos: topLeft + farCornerOffset.XProjection, rotation: MathF.PI / 2), // TopRight
            (pos: topLeft + farCornerOffset.YProjection, rotation: -MathF.PI / 2), // BottomLeft
            (pos: topLeft + farCornerOffset, rotation: MathF.PI), // BottomRight
        };

        foreach (var corner in corners)
            primitiveRenderer.DrawTriangle(
                spriteBatch,
                corner.pos,
                interiorColor,
                corner.rotation,
                layerDepth
            );
    }

    private void DrawEdgeInteriors(SpriteBatch spriteBatch, Vector2 topLeft, Vector2 interiorSize, Color interiorColor,
        float layerDepth)
    {
        const int corner = NineSliceFrame.CornerSize;
        Rectangle[] edgeSections =
        [
            new((int)(topLeft.X + corner), (int)topLeft.Y, (int)interiorSize.X, corner), // top,
            new((int)(topLeft.X + corner), (int)(topLeft.Y + corner + interiorSize.Y), (int)interiorSize.X,
                corner), // bottom
            new((int)topLeft.X, (int)(topLeft.Y + corner), corner, (int)interiorSize.Y), // left
            new((int)(topLeft.X + corner + interiorSize.X), (int)(topLeft.Y + corner), corner,
                (int)interiorSize.Y), // right
        ];

        foreach (var rectangle in edgeSections)
            primitiveRenderer.DrawRectangle(
                spriteBatch,
                rectangle,
                interiorColor,
                layerDepth
            );
    }

    private void DrawFrame(SpriteBatch spriteBatch, Vector2 topLeft, Vector2 interiorSize, Color frameColor,
        float layerDepth) => _frame.Draw(spriteBatch, topLeft, interiorSize, frameColor, layerDepth + 0.001f);
}