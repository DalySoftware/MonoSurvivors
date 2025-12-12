using System;
using ContentLibrary;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class BasicEnemySpriteSheet : ISpriteSheet
{
    private readonly Vector2 _cellSize = new(64, 64);

    public Texture2D Texture(ContentManager content) => content.Load<Texture2D>(Paths.Images.Sheets.BasicEnemy);

    public Rectangle GetFrameRectangle(Rendering.IFrame frame)
    {
        if (frame is not LookDirectionFrame directionFrame) throw new InvalidOperationException();

        var frameCoords = ToCardinalDirection(directionFrame.Direction);
        return FromCellCoords(frameCoords.x, frameCoords.y);
    }

    private static (int x, int y) ToCardinalDirection(Vector2 direction)
    {
        // Preserve explicit no-direction
        if (direction == Vector2.Zero)
            return (1, 1);

        var unitVector = Vector2.Normalize(direction);

        // Round into the 8-way grid, but allow zeros in either axis
        var x = (int)MathF.Round(unitVector.X);
        var y = (int)MathF.Round(unitVector.Y);

        // Map from direction space [-1, 1] to sheet cell space [0, 2]
        return (x + 1, y + 1);
    }

    private Rectangle FromCellCoords(int x, int y) =>
        new(x * (int)_cellSize.X, y * (int)_cellSize.Y, (int)_cellSize.X, (int)_cellSize.Y);

    private interface IFrame : Rendering.IFrame;

    public readonly record struct LookDirectionFrame(Vector2 Direction) : IFrame;
}