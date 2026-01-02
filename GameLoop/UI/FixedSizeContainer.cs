using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

internal sealed class FixedSizeContainer : IUiElement
{
    private readonly IUiElement _child;

    public FixedSizeContainer(
        Vector2 origin,
        Vector2 size,
        UiAnchor anchor,
        Func<Vector2, IUiElement> createChild,
        UiAnchor childAnchor = UiAnchor.CenterLeft)
    {
        Rectangle = new UiRectangle(origin, size, anchor);

        // Position child inside our bounds
        var childOrigin = Rectangle.AnchorForPoint(childAnchor);
        _child = createChild(childOrigin);
    }
    public UiRectangle Rectangle { get; }

    public void Draw(SpriteBatch spriteBatch) => _child.Draw(spriteBatch);
}