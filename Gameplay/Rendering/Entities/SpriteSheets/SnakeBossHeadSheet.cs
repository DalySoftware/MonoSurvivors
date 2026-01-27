using System;
using ContentLibrary;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering.SpriteSheets;

public class SnakeBossHeadSheet(ContentManager content) : ISpriteSheet
{
    internal Vector2 FrameSize { get; } = new(192, 192);
    public Texture2D Texture { get; } = content.Load<Texture2D>(Paths.Images.SnakeHead);

    public Rectangle GetFrameRectangle(Rendering.IFrame frame)
    {
        if (frame is not HeadDirectionFrame directionFrame) throw new InvalidOperationException();

        var (x, y) = CardinalDirectionHelper.ToCardinalDirection(directionFrame.Direction);
        return FromCellCoords(x, y);
    }

    private Rectangle FromCellCoords(int x, int y)
        => new(x * (int)FrameSize.X, y * (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y);

    public interface IFrame : Rendering.IFrame;

    public readonly record struct HeadDirectionFrame(Vector2 Direction) : IFrame;
}