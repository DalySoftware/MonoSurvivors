using System;
using ContentLibrary;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

internal class Button
{
    private readonly SpriteFont _font;
    private readonly Action _onClick;
    private readonly bool _rounded;
    private readonly string _text;
    private readonly Panel _panel;

    private bool _isHovered;
    private bool _isPressedVisual;


    private readonly Color _baseColor;
    private readonly Color _pressedColor;
    private readonly Color _hoveredColor;

    internal Button(ContentManager content, PrimitiveRenderer primitiveRenderer, Vector2 centre, string text,
        Action onClick, bool rounded = false)
    {
        Centre = centre;
        _text = text;
        _onClick = onClick;
        _rounded = rounded;
        _font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Medium);
        var panelRenderer = new PanelRenderer(content, primitiveRenderer);
        _panel = panelRenderer.Define(centre, PanelInteriorSize);

        _baseColor = new OklchColor(0.77f, 0.074f, 235f).ToColor();
        _pressedColor = _baseColor.ShiftLightness(-0.1f);
        _hoveredColor = _baseColor.ShiftLightness(0.1f);
    }

    internal Vector2 Centre { get; }
    internal Vector2 Size => PanelSize;

    internal Rectangle Bounds =>
        new(
            (int)(Centre.X - PanelSize.X * 0.5f),
            (int)(Centre.Y - PanelSize.Y * 0.5f),
            (int)PanelSize.X,
            (int)PanelSize.Y);


    private Vector2 PanelSize => _panel.ExteriorSize;

    private Vector2 PanelInteriorSize
    {
        get
        {
            var naiveSize = _font.MeasureString(_text);
            if (!_rounded) return naiveSize;

            var maxDimension = MathF.Max(naiveSize.X, naiveSize.Y);
            return new Vector2(maxDimension, maxDimension);
        }
    }

    internal void Focus() => _isHovered = true;
    internal void Blur() => _isHovered = false;
    internal void Hover() => _isHovered = true;
    internal void Unhover() => _isHovered = false;

    internal void PressVisual() => _isPressedVisual = true;
    internal void ReleaseVisual() => _isPressedVisual = false;
    internal void Activate() => _onClick();

    internal void Draw(SpriteBatch spriteBatch)
    {
        var color =
            _isPressedVisual ? _pressedColor :
            _isHovered ? _hoveredColor :
            _baseColor;

        // Draw panel background    
        _panel.Draw(spriteBatch, color, color.ShiftChroma(-0.04f).ShiftLightness(-.3f));

        // Draw text centered
        var textSize = _font.MeasureString(_text);
        var textPosition = _panel.Centre - textSize / 2;

        spriteBatch.DrawString(_font, _text, textPosition, layerDepth: _panel.InteriorLayerDepth + 0.01f);
    }
}