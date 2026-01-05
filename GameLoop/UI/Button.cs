using System;
using ContentLibrary;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

internal sealed class Button : IUiElement
{
    private readonly Panel _panel;
    private readonly SpriteFont _font;
    private readonly string _text;
    private readonly Action _onClick;

    private readonly Color _baseColor;
    private readonly Color _pressedColor;
    private readonly Color _hoveredColor;

    private bool _isHovered;
    private bool _isPressedVisual;

    private Button(
        ContentManager content,
        Panel panel,
        string text,
        Action onClick)
    {
        _text = text;
        _onClick = onClick;

        _font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Medium);

        _panel = panel;
        Rectangle = _panel.Rectangle;

        _baseColor = new OklchColor(0.77f, 0.074f, 235f).ToColor();
        _pressedColor = _baseColor.ShiftLightness(-0.1f);
        _hoveredColor = _baseColor.ShiftLightness(0.1f);
    }

    public UiRectangle Rectangle { get; }

    public void Draw(SpriteBatch spriteBatch)
    {
        var color =
            _isPressedVisual ? _pressedColor :
            _isHovered ? _hoveredColor :
            _baseColor;

        _panel.Draw(spriteBatch, color, color.ShiftChroma(-0.04f).ShiftLightness(-0.3f));

        var textSize = _font.MeasureString(_text);
        var textPos = _panel.Interior.CreateAnchoredRectangle(UiAnchor.Centre, textSize).TopLeft;

        spriteBatch.DrawString(_font, _text, textPos, layerDepth: _panel.InteriorLayerDepth + 0.01f);
    }

    internal void Focus() => _isHovered = true;
    internal void Blur() => _isHovered = false;
    internal void Hover() => _isHovered = true;
    internal void Unhover() => _isHovered = false;

    internal void PressVisual() => _isPressedVisual = true;
    internal void ReleaseVisual() => _isPressedVisual = false;
    internal void Activate() => _onClick();

    public sealed class Factory(
        ContentManager content,
        PrimitiveRenderer primitiveRenderer,
        string text,
        Action onClick,
        bool rounded = false)
    {
        private readonly SpriteFont _font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Medium);

        public Button Create(Vector2 origin, UiAnchor anchor)
        {
            var size = Measure();
            var interior = new UiRectangle(origin, size, anchor);
            var panel = new Panel.Factory(content, primitiveRenderer).DefineByInterior(interior);
            var button = new Button(content, panel, text, onClick);
            return button;
        }

        private Vector2 Measure()
        {
            var textSize = _font.MeasureString(text);
            var naive = new Vector2(textSize.X, textSize.Y);
            if (!rounded) return naive;

            var maxDimension = MathF.Max(naive.X, naive.Y);
            return new Vector2(maxDimension, maxDimension);
        }
    }
}