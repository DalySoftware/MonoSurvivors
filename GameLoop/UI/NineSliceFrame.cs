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

    internal Frame Define(Vector2 centre, Vector2 interiorSize, float layerDepth = 0f) =>
        new(_texture, centre, interiorSize, layerDepth);
}

internal class Frame(
    Texture2D texture,
    Vector2 centre,
    Vector2 interiorSize,
    float layerDepth)
{
    private const int EdgeLength = 8;
    private const int EdgeThickness = 28;
    private const int CornerSize = NineSliceFrame.CornerSize;

    private readonly Rectangle _topLeft = new(0, 0, CornerSize, CornerSize);
    private readonly Rectangle _topRight = new(FarCornerOrigin, 0, CornerSize, CornerSize);
    private readonly Rectangle _bottomLeft = new(0, FarCornerOrigin, CornerSize, CornerSize);
    private readonly Rectangle _bottomRight = new(FarCornerOrigin, FarCornerOrigin, CornerSize, CornerSize);

    private readonly Rectangle _topEdge = new(CornerSize, 0, EdgeLength, EdgeThickness);
    private readonly Rectangle _bottomEdge = new(CornerSize, FarEdgeOrigin, EdgeLength, EdgeThickness);
    private readonly Rectangle _leftEdge = new(0, CornerSize, EdgeThickness, EdgeLength);
    private readonly Rectangle _rightEdge = new(FarEdgeOrigin, CornerSize, EdgeThickness, EdgeLength);

    private static int TotalWidth => CornerSize * 2 + EdgeLength;
    private static int FarEdgeOrigin => TotalWidth - EdgeThickness;
    private static int FarCornerOrigin => TotalWidth - CornerSize;

    internal Vector2 TopLeft => centre - new Vector2(interiorSize.X / 2 + CornerSize, interiorSize.Y / 2 + CornerSize);

    internal Rectangle TopEdgeRectangle =>
        new((int)TopLeft.X + CornerSize, (int)TopLeft.Y, (int)interiorSize.X, CornerSize);

    internal Rectangle BottomEdgeRectangle =>
        new((int)TopLeft.X + CornerSize, (int)(TopLeft.Y + CornerSize + interiorSize.Y), (int)interiorSize.X,
            CornerSize);

    internal Rectangle LeftEdgeRectangle =>
        new((int)TopLeft.X, (int)TopLeft.Y + CornerSize, CornerSize, (int)interiorSize.Y);

    internal Rectangle RightEdgeRectangle =>
        new((int)(TopLeft.X + CornerSize + interiorSize.X), (int)TopLeft.Y + CornerSize, CornerSize,
            (int)interiorSize.Y);

    internal IEnumerable<Rectangle> EdgeRectangles =>
    [
        TopEdgeRectangle,
        BottomEdgeRectangle,
        LeftEdgeRectangle,
        RightEdgeRectangle,
    ];

    public Rectangle InteriorRectangle =>
        new((int)TopLeft.X + CornerSize, (int)TopLeft.Y + CornerSize, (int)interiorSize.X, (int)interiorSize.Y);

    public Rectangle ExteriorRectangle =>
        new((int)TopLeft.X, (int)TopLeft.Y, (int)interiorSize.X + CornerSize * 2, (int)interiorSize.Y + CornerSize * 2);

    internal IEnumerable<(Vector2 topLeft, float rotation)> CornerTriangles
    {
        get
        {
            var farCornerOffset = interiorSize + Vector2.One * CornerSize * 2f;
            return
            [
                (TopLeft, 0f),
                (TopLeft + farCornerOffset.XProjection, MathF.PI / 2),
                (TopLeft + farCornerOffset.YProjection, -MathF.PI / 2),
                (TopLeft + farCornerOffset, MathF.PI),
            ];
        }
    }

    internal void Draw(SpriteBatch spriteBatch, Color color)
    {
        var x = (int)TopLeft.X;
        var y = (int)TopLeft.Y;
        var width = (int)interiorSize.X + CornerSize * 2;
        var height = (int)interiorSize.Y + CornerSize * 2;

        DrawCorners(spriteBatch, color, x, y, width, height);
        DrawEdges(spriteBatch, color, x, y, width, height);
    }

    private void DrawEdges(SpriteBatch spriteBatch, Color color, int x, int y, int width, int height)
    {
        (Rectangle source, Rectangle destination)[] edges =
        [
            (_topEdge, new Rectangle(x + CornerSize, y, width - CornerSize * 2, EdgeThickness)),
            (_bottomEdge,
                new Rectangle(x + CornerSize, y + height - EdgeThickness, width - CornerSize * 2, EdgeThickness)),
            (_leftEdge, new Rectangle(x, y + CornerSize, EdgeThickness, height - CornerSize * 2)),
            (_rightEdge,
                new Rectangle(x + width - EdgeThickness, y + CornerSize, EdgeThickness, height - CornerSize * 2)),
        ];

        foreach (var (source, destination) in edges)
            spriteBatch.Draw(texture, destination, source, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
    }

    private void DrawCorners(SpriteBatch spriteBatch, Color color, int x, int y, int width, int height)
    {
        (Rectangle source, Rectangle destination)[] corners =
        [
            (_topLeft, new Rectangle(x, y, CornerSize, CornerSize)),
            (_topRight, new Rectangle(x + width - CornerSize, y, CornerSize, CornerSize)),
            (_bottomLeft, new Rectangle(x, y + height - CornerSize, CornerSize, CornerSize)),
            (_bottomRight, new Rectangle(x + width - CornerSize, y + height - CornerSize, CornerSize, CornerSize)),
        ];

        foreach (var (source, destination) in corners)
            spriteBatch.Draw(texture, destination, color, source, layerDepth: layerDepth);
    }
}