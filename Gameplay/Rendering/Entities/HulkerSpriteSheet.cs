using System;
using ContentLibrary;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class HulkerSpriteSheet(ContentManager content) : ISpriteSheet
{
    private readonly Vector2 _frameSize = new(128, 128);

    public Texture2D Texture { get; } = content.Load<Texture2D>(Paths.Images.Sheets.Hulker);

    public Rectangle GetFrameRectangle(Rendering.IFrame frame)
    {
        if (frame is not LookDirectionFrame directionFrame) throw new InvalidOperationException();

        var (x, y) = CardinalDirectionHelper.ToCardinalDirection(directionFrame.Direction);
        return FromCellCoords(x, y);
    }

    private Rectangle FromCellCoords(int x, int y)
        => new(x * (int)_frameSize.X, y * (int)_frameSize.Y, (int)_frameSize.X, (int)_frameSize.Y);

    public interface IFrame : Rendering.IFrame;

    public readonly record struct LookDirectionFrame(Vector2 Direction) : IFrame;
}