using System.Linq;
using ContentLibrary;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering.Tooltips;

public class ToolTipRenderer(IMouseInputState mouse, PrimitiveRenderer primitiveRenderer, ContentManager content)
{
    private readonly SpriteFont _font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Small);

    public void DrawTooltipAt(SpriteBatch spriteBatch, ToolTip tooltip, Vector2 position,
        float layerDepth = Layers.Tooltips)
    {
        var lineHeight = _font.MeasureString("A").Y;
        const int padding = 8;
        var tooltipWidth = tooltip.MaxWidth(_font) + padding * 2;

        // Draw background
        var tooltipHeight = lineHeight * tooltip.TotalLines + padding * 2;
        var tooltipRect = new Rectangle((int)position.X, (int)position.Y,
            (int)tooltipWidth, (int)tooltipHeight);
        primitiveRenderer.DrawRectangle(spriteBatch, tooltipRect, ColorPalette.Black * 0.9f, layerDepth);

        // Title
        var textPos = position + new Vector2(padding, padding);
        spriteBatch.DrawString(_font, tooltip.Title, textPos, ColorPalette.White, layerDepth: layerDepth + 0.01f);

        // Body
        foreach (var (line, index) in tooltip.Body.Select((line, i) => (line, i)))
        {
            textPos = position + new Vector2(padding, padding + (index + 1) * lineHeight);
            spriteBatch.DrawString(_font, line.Text, textPos, line.Color ?? ColorPalette.LightGray,
                layerDepth: layerDepth + 0.01f);
        }
    }

    /// <param name="layerDepth">Layer depth for background. Text will use <paramref name="layerDepth" /> + 0.01f</param>
    public void DrawTooltipAtMouse(SpriteBatch spriteBatch, ToolTip tooltip, float layerDepth = Layers.Tooltips)
    {
        var mouseState = mouse.MouseState;
        var position = new Vector2(mouseState.X + 20, mouseState.Y);

        DrawTooltipAt(spriteBatch, tooltip, position, layerDepth);
    }
}