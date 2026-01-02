using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

public readonly struct UiRectangle(Vector2 origin, Vector2 size, UiAnchor originAnchor = UiAnchor.TopLeft)
{
    public Vector2 Origin { get; } = origin; // The anchor-based origin for placement
    public Vector2 Size { get; } = size; // Width/height
    public UiAnchor OriginAnchor { get; } = originAnchor; // How the origin relates to the rectangle

    public Vector2 TopLeft { get; } =
        ComputeTopLeft(origin, size, originAnchor); // Internal: computed from origin + anchor

    public Vector2 Centre => TopLeft + Size * 0.5f;

    private static Vector2 ComputeTopLeft(Vector2 origin, Vector2 size, UiAnchor anchor)
    {
        var x = anchor switch
        {
            UiAnchor.TopLeft or UiAnchor.CenterLeft or UiAnchor.BottomLeft => origin.X,
            UiAnchor.TopCenter or UiAnchor.Centre or UiAnchor.BottomCenter => origin.X - size.X / 2f,
            UiAnchor.TopRight or UiAnchor.CenterRight or UiAnchor.BottomRight => origin.X - size.X,
            _ => throw new ArgumentOutOfRangeException(nameof(anchor)),
        };

        var y = anchor switch
        {
            UiAnchor.TopLeft or UiAnchor.TopCenter or UiAnchor.TopRight => origin.Y,
            UiAnchor.CenterLeft or UiAnchor.Centre or UiAnchor.CenterRight => origin.Y - size.Y / 2f,
            UiAnchor.BottomLeft or UiAnchor.BottomCenter or UiAnchor.BottomRight => origin.Y - size.Y,
            _ => throw new ArgumentOutOfRangeException(nameof(anchor)),
        };

        return new Vector2(x, y);
    }

    // Returns a child rectangle at a given anchor and size
    public UiRectangle CreateAnchoredRectangle(UiAnchor anchor, Vector2 childSize, Vector2? offset = null)
    {
        offset ??= Vector2.Zero;
        return new UiRectangle(AnchorForPoint(anchor) + offset.Value, childSize, anchor);
    }

    /// <summary>
    ///     Returns the top-left position corresponding to an anchor for a **point**, not a rectangle.
    ///     Works regardless of rectangle origin anchor.
    /// </summary>
    public Vector2 AnchorForPoint(UiAnchor anchor)
    {
        var offset = anchor switch
        {
            UiAnchor.TopLeft => Vector2.Zero,
            UiAnchor.TopCenter => new Vector2(Size.X / 2f, 0),
            UiAnchor.TopRight => new Vector2(Size.X, 0),
            UiAnchor.CenterLeft => new Vector2(0, Size.Y / 2f),
            UiAnchor.Centre => Size * 0.5f,
            UiAnchor.CenterRight => new Vector2(Size.X, Size.Y / 2f),
            UiAnchor.BottomLeft => new Vector2(0, Size.Y),
            UiAnchor.BottomCenter => new Vector2(Size.X / 2f, Size.Y),
            UiAnchor.BottomRight => Size,
            _ => throw new ArgumentOutOfRangeException(nameof(anchor)),
        };

        return TopLeft + offset;
    }

    public bool Contains(Vector2 point) =>
        point.X >= TopLeft.X &&
        point.X <= TopLeft.X + Size.X &&
        point.Y >= TopLeft.Y &&
        point.Y <= TopLeft.Y + Size.Y;

    public Rectangle ToRectangle() => new(TopLeft.ToPoint(), Size.ToPoint());
}

public interface IUiElement
{
    /// <summary>Fully positioned and sized rectangle of this element.</summary>
    UiRectangle Rectangle { get; }

    /// <summary>Draw the element.</summary>
    void Draw(SpriteBatch spriteBatch);
}

public enum UiAnchor
{
    TopLeft,
    TopCenter,
    TopRight,
    CenterLeft,
    Centre,
    CenterRight,
    BottomLeft,
    BottomCenter,
    BottomRight,
}

public static class UiExtensions
{
    extension(Viewport viewport)
    {
        public UiRectangle UiRectangle() => new(
            Vector2.Zero,
            new Vector2(viewport.Width, viewport.Height)
        );
    }
}