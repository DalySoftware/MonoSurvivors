using ContentLibrary;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

public class PanelRenderer(ContentManager content)
{
    private const int CornerSize = 28;
    private const int EdgeLength = 8;
    private const int EdgeThickness = 28;
    private readonly static int FarEdgeOrigin = TotalWidth - EdgeThickness;
    private readonly static int FarCornerOrigin = TotalWidth - CornerSize;

    private readonly Rectangle _bottomEdge =
        new(CornerSize, FarEdgeOrigin, EdgeLength, EdgeThickness);
    private readonly Rectangle _bottomLeft = new(0, FarCornerOrigin, CornerSize, CornerSize);
    private readonly Rectangle _bottomRight =
        new(FarCornerOrigin, FarCornerOrigin, CornerSize, CornerSize);
    private readonly Rectangle _centre = new(CornerSize, CornerSize, EdgeLength,
        EdgeLength);
    private readonly Rectangle _leftEdge = new(0, CornerSize, EdgeThickness, EdgeLength);
    private readonly Rectangle _rightEdge = new(FarEdgeOrigin, CornerSize, EdgeThickness, EdgeLength);
    private readonly Texture2D _texture = content.Load<Texture2D>(Paths.Images.PanelNineSlice);
    private readonly Rectangle _topEdge = new(CornerSize, 0, EdgeLength, EdgeThickness);
    private readonly Rectangle _topLeft = new(0, 0, CornerSize, CornerSize);
    private readonly Rectangle _topRight = new(FarCornerOrigin, 0, CornerSize, CornerSize);
    private static int TotalWidth => CornerSize * 2 + EdgeLength;

    public static Vector2 GetCenter(Vector2 position, Vector2 interiorSize)
    {
        var width = (int)interiorSize.X + CornerSize * 2;
        var height = (int)interiorSize.Y + CornerSize * 2;
        return new Vector2(position.X + width / 2f, position.Y + height / 2f);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Vector2 interiorSize, Color? color = null,
        float layerDepth = 0f)
    {
        var clr = color ?? Color.White; // Compile doing weird stuff if I use the nullable param directly 

        var x = (int)position.X;
        var y = (int)position.Y;
        var width = (int)interiorSize.X + CornerSize * 2;
        var height = (int)interiorSize.Y + CornerSize * 2;

        // Corners
        spriteBatch.Draw(_texture, new Rectangle(x, y, CornerSize, CornerSize), clr, _topLeft, layerDepth: layerDepth);
        spriteBatch.Draw(_texture, new Rectangle(x + width - CornerSize, y, CornerSize, CornerSize),
            clr, _topRight, layerDepth: layerDepth);
        spriteBatch.Draw(_texture, new Rectangle(x, y + height - CornerSize, CornerSize, CornerSize),
            clr, _bottomLeft, layerDepth: layerDepth);
        spriteBatch.Draw(_texture,
            new Rectangle(x + width - CornerSize, y + height - CornerSize, CornerSize, CornerSize),
            clr, _bottomRight, layerDepth: layerDepth);

        // Top and bottom edges
        spriteBatch.Draw(_texture, new Rectangle(x + CornerSize, y, width - CornerSize * 2, EdgeThickness),
            clr, _topEdge, layerDepth: layerDepth);
        spriteBatch.Draw(_texture,
            new Rectangle(x + CornerSize, y + height - EdgeThickness, width - CornerSize * 2, EdgeThickness),
            clr, _bottomEdge, layerDepth: layerDepth);

        // Left and right edges
        spriteBatch.Draw(_texture, new Rectangle(x, y + CornerSize, EdgeThickness, height - CornerSize * 2),
            clr, _leftEdge, layerDepth: layerDepth);
        spriteBatch.Draw(_texture,
            new Rectangle(x + width - EdgeThickness, y + CornerSize, EdgeThickness,
                height - CornerSize * 2),
            clr, _rightEdge, layerDepth: layerDepth);

        // Centre panel
        spriteBatch.Draw(_texture,
            new Rectangle(x + CornerSize, y + CornerSize, width - CornerSize * 2,
                height - CornerSize * 2),
            clr, _centre, layerDepth: layerDepth);
    }
}