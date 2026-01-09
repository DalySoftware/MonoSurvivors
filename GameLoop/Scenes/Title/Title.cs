using System;
using ContentLibrary;
using GameLoop.UI;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Title;

internal class Title(SpriteFont font, UiRectangle rectangle) : IUiElement
{
    private const string Line1 = "Veil of";
    private const string Line2 = "Cataclysm";
    private const float Line2Offset = 150f; // This isn't a gap but TopLeft to TopLeft. Could be refactored further
    private readonly static Color ForegroundColor = ColorPalette.Teal;
    private readonly static Color ShadowColor = ColorPalette.Teal.ShiftChroma(-0.04f).ShiftLightness(-0.32f);

    public UiRectangle Rectangle { get; } = rectangle;

    public void Draw(SpriteBatch spriteBatch)
    {
        var shadowOffset = new Vector2(-10f, 10f);

        var line1Rectangle = Rectangle.CreateAnchoredRectangle(UiAnchor.TopCenter, font.MeasureString(Line1));
        spriteBatch.DrawString(font, Line1, line1Rectangle.TopLeft, ForegroundColor, layerDepth: Layers.Front);
        spriteBatch.DrawString(font, Line1, line1Rectangle.TopLeft + shadowOffset, ShadowColor,
            layerDepth: Layers.Shadow);

        var line2Rectangle =
            Rectangle.CreateAnchoredRectangle(UiAnchor.BottomCenter, font.MeasureString(Line2));
        spriteBatch.DrawString(font, Line2, line2Rectangle.TopLeft, ForegroundColor, layerDepth: Layers.Front);
        spriteBatch.DrawString(font, Line2, line2Rectangle.TopLeft + shadowOffset, ShadowColor,
            layerDepth: Layers.Shadow);
    }

    internal class Factory(ContentManager content)
    {
        private readonly SpriteFont _font = content.Load<SpriteFont>(Paths.Fonts.Righteous.Large);

        public Title Create(Vector2 anchor)
        {
            var size = Measure();
            var rectangle = new UiRectangle(anchor, size, UiAnchor.TopCenter);
            return new Title(_font, rectangle);
        }

        private Vector2 Measure()
        {
            var line1Size = _font.MeasureString(Line1);
            var line2Size = _font.MeasureString(Line2);

            var wider = MathF.Max(line1Size.X, line2Size.X);
            return new Vector2(wider, line1Size.Y + Line2Offset);
        }
    }

    private static class Layers
    {
        internal const float Shadow = 0.4f;
        internal const float Front = 0.6f;
    }
}