using System;
using ContentLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.UI;

public class Button : UiElement
{
    private readonly SpriteFont _font;
    private readonly string _text;
    private readonly Action _onClick;
    private readonly Vector2 _size;
    private readonly PanelRenderer _panelRenderer;

    private bool _isHovered;
    private bool _wasPressed;

    public Button(ContentManager content, Vector2 position, Vector2 size, string text, Action onClick)
    {
        Position = position;
        _size = size;
        _text = text;
        _onClick = onClick;
        _font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Medium);
        _panelRenderer = new PanelRenderer(content);
    }

    public void Update(MouseState mouseState, MouseState previousMouseState)
    {
        if (!IsVisible) return;

        var bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)_size.X, (int)_size.Y);
        _isHovered = bounds.Contains(mouseState.Position);

        if (_isHovered && mouseState.LeftButton == ButtonState.Pressed)
        {
            _wasPressed = true;
        }
        else if (_wasPressed && mouseState.LeftButton == ButtonState.Released && _isHovered)
        {
            _onClick();
            _wasPressed = false;
        }
        else if (mouseState.LeftButton == ButtonState.Released)
        {
            _wasPressed = false;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        var color = _wasPressed ? new Color(180, 180, 180) : (_isHovered ? new Color(220, 220, 220) : Color.White);

        // Draw panel background
        var interiorSize = new Vector2(_size.X - 56, _size.Y - 56); // Account for panel borders
        _panelRenderer.Draw(spriteBatch, Position, interiorSize, color, layerDepth: 0.1f);

        // Draw text centered
        var textSize = _font.MeasureString(_text);
        var panelCenter = PanelRenderer.GetCenter(Position, interiorSize);
        var textPosition = panelCenter - textSize / 2;

        spriteBatch.DrawString(_font, _text, textPosition, Color.Black, 0f, Vector2.Zero, 1f,
            SpriteEffects.None, 0.05f);
    }
}
