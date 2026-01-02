using System;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

internal sealed class Label : IUiElement
{
    private readonly SpriteFont _font;
    private readonly TextAlignment _alignment;
    private readonly string _templateString;

    private readonly Color _color;
    private readonly float _layerDepth;

    private Vector2 _textSize;


    private Label(
        SpriteFont font,
        Vector2 origin,
        UiAnchor anchor,
        string text,
        Color color,
        TextAlignment alignment = TextAlignment.Left,
        string templateString = "", // Reserves at least the space for this
        float layerDepth = 0f)
    {
        _font = font;
        _alignment = alignment;
        _templateString = templateString;
        Text = text;
        _color = color;
        _layerDepth = layerDepth;

        UpdateRectangle(origin, anchor);
    }

    public string Text
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            UpdateRectangle(Rectangle.Origin, Rectangle.OriginAnchor);
        }
    }


    public UiRectangle Rectangle { get; private set; }

    public void Draw(SpriteBatch spriteBatch)
    {
        var position = Rectangle.TopLeft;
        var origin = Vector2.Zero;

        if (_alignment == TextAlignment.Right)
        {
            position.X += Rectangle.Size.X;
            origin.X = _textSize.X;
        }

        spriteBatch.DrawString(_font, Text, position, _color, origin: origin, layerDepth: _layerDepth);
    }


    private void UpdateRectangle(Vector2 origin, UiAnchor anchor)
    {
        _textSize = _font.MeasureString(Text);
        var rectSize = _textSize;

        if (!string.IsNullOrEmpty(_templateString))
        {
            var templateWidth = _font.MeasureString(_templateString).X;
            rectSize.X = MathF.Max(rectSize.X, templateWidth);
        }

        Rectangle = new UiRectangle(origin, rectSize, anchor);
    }


    public sealed class Factory(
        ContentManager content,
        string fontPath,
        string text,
        Color? color = null,
        TextAlignment alignment = TextAlignment.Left,
        string templateString = "", // Reserves at least the space for this
        float layerDepth = 0f)
    {
        private readonly SpriteFont _font = content.Load<SpriteFont>(fontPath);

        private readonly Color _color = color ?? Color.White;

        public Label Create(Vector2 origin, UiAnchor anchor) =>
            new(_font, origin, anchor, text, _color, alignment, templateString, layerDepth);
    }
}

public enum TextAlignment
{
    Left,
    Right,
}