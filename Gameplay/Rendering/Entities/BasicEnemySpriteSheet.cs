using System;
using ContentLibrary;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class BasicEnemySpriteSheet(ContentManager content) : ISpriteSheet
{
    private readonly Vector2 _cellSize = new(64, 64);

    public Texture2D Texture { get; } = content.Load<Texture2D>(Paths.Images.Sheets.BasicEnemy);

    public Rectangle GetFrameRectangle(Rendering.IFrame frame)
    {
        if (frame is not LookDirectionFrame directionFrame) throw new InvalidOperationException();

        var (x, y) = CardinalDirectionHelper.ToCardinalDirection(directionFrame.Direction);
        return FromCellCoords(x, y);
    }

    private Rectangle FromCellCoords(int x, int y) =>
        new(x * (int)_cellSize.X, y * (int)_cellSize.Y, (int)_cellSize.X, (int)_cellSize.Y);

    private interface IFrame : Rendering.IFrame;

    public class LookDirectionFrame(Vector2 direction) : IFrame
    {
        public Vector2 Direction { get; set; } = direction;
    }
}