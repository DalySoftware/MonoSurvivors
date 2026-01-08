using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

public abstract class UiStack(Vector2 origin, float gap, UiAnchor originAnchor) : IUiElement
{
    private readonly List<IUiElement> _children = [];
    private Vector2 _nextOffset = Vector2.Zero;

    protected Vector2 Origin { get; } = origin;
    protected UiAnchor OriginAnchor { get; } = originAnchor;

    protected float Gap { get; } = gap;

    public UiRectangle Rectangle { get; private set; } = new(origin, Vector2.Zero, originAnchor);

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var child in _children)
            child.Draw(spriteBatch);
    }

    public T AddChild<T>(Func<Vector2, T> create) where T : IUiElement
    {
        var childAnchor = ComputeChildAnchor(_nextOffset);
        var child = create(childAnchor);
        _children.Add(child);

        _nextOffset = UpdateOffset(_nextOffset, child.Rectangle.Size);
        Rectangle = new UiRectangle(Origin, ComputeTotalSize(), OriginAnchor);

        return child;
    }

    private Vector2 ComputeTotalSize()
    {
        if (_children.Count == 0)
            return Vector2.Zero;

        var min = new Vector2(float.MaxValue);
        var max = new Vector2(float.MinValue);

        foreach (var child in _children)
        {
            var rect = child.Rectangle;

            var tl = rect.AnchorForPoint(UiAnchor.TopLeft);
            var br = rect.AnchorForPoint(UiAnchor.BottomRight);

            min = Vector2.Min(min, tl);
            max = Vector2.Max(max, br);
        }

        return max - min;
    }


    protected abstract Vector2 UpdateOffset(Vector2 currentOffset, Vector2 childSize);
    protected abstract Vector2 ComputeChildAnchor(Vector2 offset);
}

public sealed class VerticalStack(Vector2 topCenter, float gap)
    : UiStack(topCenter, gap, UiAnchor.TopCenter)
{
    protected override Vector2 UpdateOffset(Vector2 currentOffset, Vector2 childSize) =>
        currentOffset + new Vector2(0, childSize.Y + Gap);

    protected override Vector2 ComputeChildAnchor(Vector2 offset) =>
        new(
            Origin.X, // fixed horizontal center line
            Origin.Y + offset.Y // stacked vertically along Y
        );
}

public sealed class HorizontalStack(Vector2 centerLeft, float gap)
    : UiStack(centerLeft, gap, UiAnchor.CenterLeft)
{
    protected override Vector2 UpdateOffset(Vector2 currentOffset, Vector2 childSize) =>
        currentOffset + new Vector2(childSize.X + Gap, 0);

    protected override Vector2 ComputeChildAnchor(Vector2 offset) =>
        new(
            Origin.X + offset.X, // stacked horizontally along X
            Origin.Y // fixed vertical center line
        );
}