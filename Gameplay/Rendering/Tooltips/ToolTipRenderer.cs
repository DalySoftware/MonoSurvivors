using System.Linq;
using ContentLibrary;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Gameplay.Rendering.Tooltips;

public class ToolTipRenderer(PrimitiveRenderer primitiveRenderer, ContentManager content)
{
    private readonly SpriteFont _font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Small);

    /// <param name="layerDepth">Layer depth for background. Text will use <paramref name="layerDepth" /> + 0.01f</param>
    public void DrawTooltip(SpriteBatch spriteBatch, ToolTip tooltip, float layerDepth = Layers.Tooltips)
    {
        var mouseState = Mouse.GetState();
        var tooltipPos = new Vector2(mouseState.X + 20, mouseState.Y);

        var lineHeight = _font.MeasureString("A").Y;
        const int padding = 8;
        var tooltipWidth = tooltip.MaxWidth(_font) + padding * 2;

        // Draw background
        var tooltipHeight = lineHeight * tooltip.TotalLines + padding * 2;
        var tooltipRect = new Rectangle((int)tooltipPos.X, (int)tooltipPos.Y,
            (int)tooltipWidth, (int)tooltipHeight);
        primitiveRenderer.DrawRectangle(spriteBatch, tooltipRect, Color.Black * 0.9f, layerDepth);

        // Title
        var textPos = tooltipPos + new Vector2(padding, padding);
        spriteBatch.DrawString(_font, tooltip.Title, textPos, Color.White, layerDepth: layerDepth + 0.01f);

        // Body
        foreach (var (line, index) in tooltip.Body.Select((line, i) => (line, i)))
        {
            textPos = tooltipPos + new Vector2(padding, padding + (index + 1) * lineHeight);
            spriteBatch.DrawString(_font, line.Text, textPos, line.Color ?? Color.LightGray,
                layerDepth: layerDepth + 0.01f);
        }
    }
}