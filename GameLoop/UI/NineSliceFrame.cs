using System;
using System.Collections.Generic;
using ContentLibrary;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

internal sealed class NineSliceFrame(ContentManager content)
{
    public const int CornerSize = 28;
    private readonly Texture2D _texture = content.Load<Texture2D>(Paths.Images.PanelNineSlice);

    internal Frame Define(UiRectangle exterior, float layerDepth) => new(_texture, exterior, layerDepth);
}

internal class Frame(
    Texture2D texture,
    UiRectangle exterior,
    float layerDepth)
{
    private const int EdgeLength = 8;
    private const int EdgeThickness = NineSliceFrame.CornerSize;
    private const int CornerSize = NineSliceFrame.CornerSize;

    private readonly Rectangle _topLeft = new(0, 0, CornerSize, CornerSize);
    private readonly Rectangle _topRight = new(FarCornerOrigin, 0, CornerSize, CornerSize);
    private readonly Rectangle _bottomLeft = new(0, FarCornerOrigin, CornerSize, CornerSize);
    private readonly Rectangle _bottomRight = new(FarCornerOrigin, FarCornerOrigin, CornerSize, CornerSize);

    private readonly Rectangle _topEdge = new(CornerSize, 0, EdgeLength, EdgeThickness);
    private readonly Rectangle _bottomEdge = new(CornerSize, FarEdgeOrigin, EdgeLength, EdgeThickness);
    private readonly Rectangle _leftEdge = new(0, CornerSize, EdgeThickness, EdgeLength);
    private readonly Rectangle _rightEdge = new(FarEdgeOrigin, CornerSize, EdgeThickness, EdgeLength);

    internal float LayerDepth => layerDepth;

    private static int TotalWidth => CornerSize * 2 + EdgeLength;
    private static int FarEdgeOrigin => TotalWidth - EdgeThickness;
    private static int FarCornerOrigin => TotalWidth - CornerSize;

    // Interior rectangle (content area)
    public UiRectangle Interior => new(
        exterior.TopLeft + Vector2.One * CornerSize,
        exterior.Size - Vector2.One * CornerSize * 2
    );

    // Edge rectangles relative to the exterior rectangle
    internal Rectangle TopEdgeRectangle =>
        exterior.CreateAnchoredRectangle(UiAnchor.TopCenter, new Vector2(Interior.Size.X, CornerSize))
            .ToRectangle();

    internal Rectangle BottomEdgeRectangle =>
        exterior.CreateAnchoredRectangle(UiAnchor.BottomCenter, new Vector2(Interior.Size.X, CornerSize))
            .ToRectangle();

    internal Rectangle LeftEdgeRectangle =>
        exterior.CreateAnchoredRectangle(UiAnchor.CenterLeft, new Vector2(CornerSize, Interior.Size.Y))
            .ToRectangle();

    internal Rectangle RightEdgeRectangle =>
        exterior.CreateAnchoredRectangle(UiAnchor.CenterRight, new Vector2(CornerSize, Interior.Size.Y))
            .ToRectangle();

    internal IEnumerable<Rectangle> EdgeRectangles =>
    [
        TopEdgeRectangle,
        BottomEdgeRectangle,
        LeftEdgeRectangle,
        RightEdgeRectangle,
    ];

    public Rectangle InteriorRectangle => Interior.ToRectangle();
    public Rectangle ExteriorRectangle => exterior.ToRectangle();

    internal (Vector2 topLeft, float rotation) TopLeftTriangle => Corner(UiAnchor.TopLeft, 0f);
    internal (Vector2 topLeft, float rotation) TopRightTriangle => Corner(UiAnchor.TopRight, MathF.PI / 2);
    internal (Vector2 topLeft, float rotation) BottomLeftTriangle => Corner(UiAnchor.BottomLeft, -MathF.PI / 2);
    internal (Vector2 topLeft, float rotation) BottomRightTriangle => Corner(UiAnchor.BottomRight, MathF.PI);

    internal IEnumerable<(Vector2 topLeft, float rotation)> CornerTriangles =>
    [
        TopLeftTriangle, TopRightTriangle, BottomLeftTriangle, BottomRightTriangle,
    ];

    private (Vector2 topLeft, float rotation) Corner(UiAnchor anchor, float rotation)
    {
        // Rotation is applied clockwise around top-left corner.
        var pos = exterior.AnchorForPoint(anchor);
        return (pos, rotation);
    }

    internal void Draw(SpriteBatch spriteBatch, Color color)
    {
        DrawCorners(spriteBatch, color);
        DrawEdges(spriteBatch, color);
    }

    private void DrawEdges(SpriteBatch spriteBatch, Color color)
    {
        var exteriorRect = ExteriorRectangle;
        var interiorRect = InteriorRectangle;

        (Rectangle source, Rectangle destination)[] edges =
        [
            (_topEdge, new Rectangle(interiorRect.X, exteriorRect.Y, interiorRect.Width, EdgeThickness)),
            (_bottomEdge,
                new Rectangle(interiorRect.X, interiorRect.Y + interiorRect.Height, interiorRect.Width, EdgeThickness)),
            (_leftEdge, new Rectangle(exteriorRect.X, interiorRect.Y, EdgeThickness, interiorRect.Height)),
            (_rightEdge,
                new Rectangle(interiorRect.X + interiorRect.Width, interiorRect.Y, EdgeThickness, interiorRect.Height)),
        ];

        foreach (var (source, destination) in edges)
            spriteBatch.Draw(texture, destination, source, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
    }

    private void DrawCorners(SpriteBatch spriteBatch, Color color)
    {
        var rect = ExteriorRectangle;

        (Rectangle source, Rectangle destination)[] corners =
        [
            (_topLeft, new Rectangle(rect.X, rect.Y, CornerSize, CornerSize)),
            (_topRight, new Rectangle(rect.Right - CornerSize, rect.Y, CornerSize, CornerSize)),
            (_bottomLeft, new Rectangle(rect.X, rect.Bottom - CornerSize, CornerSize, CornerSize)),
            (_bottomRight, new Rectangle(rect.Right - CornerSize, rect.Bottom - CornerSize, CornerSize, CornerSize)),
        ];

        foreach (var (source, destination) in corners)
            spriteBatch.Draw(texture, destination, color, source, layerDepth: layerDepth);
    }
}