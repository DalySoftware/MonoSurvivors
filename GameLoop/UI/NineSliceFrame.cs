using ContentLibrary;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

public sealed class NineSliceFrame(ContentManager content)
{
    public const int CornerSize = 28;
    private const int EdgeLength = 8;
    private const int EdgeThickness = 28;

    private readonly Texture2D _texture = content.Load<Texture2D>(Paths.Images.PanelNineSlice);

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

    public static Rectangle GetInteriorRect(Vector2 position, Vector2 interiorSize)
        => new(
            (int)position.X + CornerSize,
            (int)position.Y + CornerSize,
            (int)interiorSize.X,
            (int)interiorSize.Y
        );

    public static Rectangle GetExteriorRect(Vector2 position, Vector2 interiorSize)
        => new(
            (int)position.X,
            (int)position.Y,
            (int)interiorSize.X + CornerSize * 2,
            (int)interiorSize.Y + CornerSize * 2
        );

    public void Draw(
        SpriteBatch spriteBatch,
        Vector2 position,
        Vector2 interiorSize,
        Color color,
        float layerDepth = 0f)
    {
        var x = (int)position.X;
        var y = (int)position.Y;
        var width = (int)interiorSize.X + CornerSize * 2;
        var height = (int)interiorSize.Y + CornerSize * 2;

        DrawCorners(spriteBatch, color, layerDepth, x, y, width, height);
        DrawEdges(spriteBatch, color, layerDepth, x, y, width, height);
    }

    private void DrawEdges(SpriteBatch spriteBatch, Color color, float layerDepth, int x, int y, int width, int height)
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
            spriteBatch.Draw(_texture, destination, source, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
    }

    private void DrawCorners(SpriteBatch spriteBatch, Color color, float layerDepth, int x, int y, int width,
        int height)
    {
        (Rectangle source, Rectangle destination)[] corners =
        [
            (_topLeft, new Rectangle(x, y, CornerSize, CornerSize)),
            (_topRight, new Rectangle(x + width - CornerSize, y, CornerSize, CornerSize)),
            (_bottomLeft, new Rectangle(x, y + height - CornerSize, CornerSize, CornerSize)),
            (_bottomRight, new Rectangle(x + width - CornerSize, y + height - CornerSize, CornerSize, CornerSize)),
        ];

        foreach (var (source, destination) in corners)
            spriteBatch.Draw(_texture, destination, color, source, layerDepth: layerDepth);
    }
}