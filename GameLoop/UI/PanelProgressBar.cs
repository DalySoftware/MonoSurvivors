using System;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

internal sealed class PanelProgressBar(
    Panel panel,
    PrimitiveRenderer primitiveRenderer,
    Color frameColor,
    Color backgroundColor,
    Color fillColor)
{
    private const float Epsilon = 0.0001f;
    internal float Start { get; set; } // 0..1
    internal float End { get; set; } // 0..1
    private bool TouchesLeft => Start <= Epsilon;
    private bool TouchesRight => End >= 1f - Epsilon;

    internal Rectangle InteriorRectangle { get; } = panel.Frame.InteriorRectangle;
    internal float FillLayerDepth { get; } = (panel.InteriorLayerDepth + panel.Frame.LayerDepth) * 0.5f;
    private Frame Frame => panel.Frame;

    internal void Draw(SpriteBatch spriteBatch)
    {
        // Convert to pixels in interior space
        var interior = InteriorRectangle;

        var startX = interior.X + (int)MathF.Round(interior.Width * Start);
        var endX = interior.X + (int)MathF.Round(interior.Width * End);

        var fillWidth = Math.Max(0, endX - startX);
        if (fillWidth > 0)
        {
            DrawInterior(spriteBatch, interior, startX, fillWidth);
            DrawEdges(spriteBatch, startX, fillWidth);
            DrawCorners(spriteBatch, fillWidth);
        }

        panel.Draw(spriteBatch, frameColor, backgroundColor);
    }

    private void DrawInterior(SpriteBatch spriteBatch, Rectangle interiorRect, int startX, int fillWidth)
    {
        var filledCenterRect = new Rectangle(startX, interiorRect.Y, fillWidth, interiorRect.Height);
        primitiveRenderer.DrawRectangle(spriteBatch, filledCenterRect, fillColor, FillLayerDepth);
    }

    private void DrawEdges(SpriteBatch spriteBatch, int startX, int fillWidth)
    {
        // Top/bottom strips inside the rounded frame area
        var topEdge = Frame.TopEdgeRectangle;
        primitiveRenderer.DrawRectangle(
            spriteBatch,
            new Rectangle(startX, topEdge.Y, fillWidth, topEdge.Height),
            fillColor,
            FillLayerDepth);

        var bottomEdge = Frame.BottomEdgeRectangle;
        primitiveRenderer.DrawRectangle(
            spriteBatch,
            new Rectangle(startX, bottomEdge.Y, fillWidth, bottomEdge.Height),
            fillColor,
            FillLayerDepth);

        // Only fill the rounded left cap if the segment actually touches the left end.
        if (TouchesLeft)
        {
            var leftEdge = Frame.LeftEdgeRectangle;
            primitiveRenderer.DrawRectangle(spriteBatch, leftEdge, fillColor, FillLayerDepth);
        }

        // Only fill the rounded right cap if the segment touches the right end.
        if (TouchesRight)
        {
            var rightEdge = Frame.RightEdgeRectangle;
            primitiveRenderer.DrawRectangle(spriteBatch, rightEdge, fillColor, FillLayerDepth);
        }
    }

    private void DrawCorners(SpriteBatch spriteBatch, int fillWidth)
    {
        // Avoid per-frame list allocs; just draw what applies.
        if (fillWidth <= 0) return;

        if (TouchesLeft)
        {
            var (p0, r0) = Frame.TopLeftTriangle;
            primitiveRenderer.DrawTriangle(spriteBatch, p0, fillColor, r0, FillLayerDepth);

            var (p1, r1) = Frame.BottomLeftTriangle;
            primitiveRenderer.DrawTriangle(spriteBatch, p1, fillColor, r1, FillLayerDepth);
        }

        if (TouchesRight)
        {
            var (p0, r0) = Frame.TopRightTriangle;
            primitiveRenderer.DrawTriangle(spriteBatch, p0, fillColor, r0, FillLayerDepth);

            var (p1, r1) = Frame.BottomRightTriangle;
            primitiveRenderer.DrawTriangle(spriteBatch, p1, fillColor, r1, FillLayerDepth);
        }
    }
}