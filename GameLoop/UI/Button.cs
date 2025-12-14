using System;
using ContentLibrary;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.UI;

internal class Button
{
    private readonly SpriteFont _font;
    private readonly Action _onClick;
    private readonly PanelRenderer _panelRenderer;
    private readonly bool _rounded;
    private readonly string _text;

    private bool _isHovered;
    private bool _wasPressed;

    internal Button(ContentManager content, Vector2 centre, string text, Action onClick, bool rounded = false)
    {
        Centre = centre;
        _text = text;
        _onClick = onClick;
        _rounded = rounded;
        _font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Medium);
        _panelRenderer = new PanelRenderer(content);
    }

    internal Vector2 Centre { get; set; }
    internal Vector2 Size => PanelSize;
    internal Vector2 TopLeft => Centre - PanelSize * 0.5f;
    internal Vector2 BottomRight => Centre + PanelSize * 0.5f;

    private Vector2 PanelSize =>
        PanelRenderer.GetExteriorSize(PanelInteriorSize);

    private Vector2 PanelInteriorSize
    {
        get
        {
            var naiveSize = _font.MeasureString(_text);
            if (!_rounded) return naiveSize;

            var negativePadding = new Vector2(0, 16); // The current font reserves way too much line height
            var maxDimension = MathF.Max(naiveSize.X, naiveSize.Y);
            return new Vector2(maxDimension, maxDimension);
        }
    }


    internal void Update(MouseState mouseState)
    {
        var bounds = new Rectangle(
            (int)TopLeft.X,
            (int)TopLeft.Y,
            (int)PanelSize.X,
            (int)PanelSize.Y);

        _isHovered = bounds.Contains(mouseState.Position);

        var mouseDown = mouseState.LeftButton == ButtonState.Pressed;
        var mouseUp = mouseState.LeftButton == ButtonState.Released;
        var click = mouseUp && _wasPressed && _isHovered;

        if (mouseDown && _isHovered)
            _wasPressed = true;

        if (click)
            _onClick();

        if (mouseUp)
            _wasPressed = false;
    }


    internal void Draw(SpriteBatch spriteBatch)
    {
        var baseTint = new OklchColor(0.77f, 0.074f, 235f).ToColor();
        var color = _wasPressed ? baseTint.ShiftLightness(-0.1f) :
            _isHovered ? baseTint.ShiftLightness(0.1f) :
            baseTint;

        // Draw panel background
        _panelRenderer.Draw(spriteBatch, TopLeft, PanelInteriorSize, color, 0.4f);

        // Draw text centered
        var textSize = _font.MeasureString(_text);
        var panelCenter = PanelRenderer.GetCenter(TopLeft, PanelInteriorSize);
        var textPosition = panelCenter - textSize / 2;

        spriteBatch.DrawString(_font, _text, textPosition, Color.White, 0f, Vector2.Zero, 1f,
            SpriteEffects.None, 0.5f);
    }
}