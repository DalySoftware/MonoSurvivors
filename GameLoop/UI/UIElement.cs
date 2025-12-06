using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

/// <summary>
///     Base class for all UI elements
/// </summary>
public abstract class UiElement
{
    public Vector2 Position { get; set; }
    public bool IsVisible { get; set; } = true;

    public abstract void Draw(SpriteBatch spriteBatch);
}