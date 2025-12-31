using System;
using System.Collections.Generic;
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
    internal float Progress { get; set; } // 0..1
    internal Rectangle InteriorRectangle { get; } = panel.Frame.InteriorRectangle;
    internal float FillLayerDepth { get; } = (panel.InteriorLayerDepth + panel.Frame.LayerDepth) * 0.5f;
    private Frame Frame => panel.Frame;

    internal void Draw(SpriteBatch spriteBatch)
    {
        var filledWidth = (int)(InteriorRectangle.Width * MathHelper.Clamp(Progress, 0f, 1f));

        DrawInterior(spriteBatch, InteriorRectangle, filledWidth);
        DrawEdges(spriteBatch, InteriorRectangle, filledWidth);
        DrawCorners(spriteBatch, InteriorRectangle, filledWidth);

        panel.Draw(spriteBatch, frameColor, backgroundColor);
    }

    private void DrawInterior(SpriteBatch spriteBatch, Rectangle interiorRect, int filledWidth)
    {
        var filledCenterRect = new Rectangle(interiorRect.X, interiorRect.Y, filledWidth, interiorRect.Height);
        primitiveRenderer.DrawRectangle(spriteBatch, filledCenterRect, fillColor, FillLayerDepth);
    }

    private void DrawEdges(SpriteBatch spriteBatch, Rectangle interiorRect, int filledWidth)
    {
        var topEdge = Frame.TopEdgeRectangle;
        var topInterior = new Rectangle(topEdge.X, topEdge.Y, Math.Min((int)(topEdge.Width * Progress), filledWidth),
            topEdge.Height);

        var bottomEdge = Frame.BottomEdgeRectangle;
        var bottomInterior = new Rectangle(bottomEdge.X, bottomEdge.Y,
            Math.Min((int)(bottomEdge.Width * Progress), filledWidth), bottomEdge.Height);

        IEnumerable<Rectangle> toDraw = [topInterior, bottomInterior];
        foreach (var rectangle in toDraw)
            primitiveRenderer.DrawRectangle(spriteBatch, rectangle, fillColor, FillLayerDepth);

        if (filledWidth > 0)
        {
            var leftEdge = Frame.LeftEdgeRectangle;
            primitiveRenderer.DrawRectangle(spriteBatch, leftEdge, fillColor, FillLayerDepth);
        }

        // Only draw right edge once the fill reaches it
        var rightEdge = Frame.RightEdgeRectangle;
        var rightEdgeStartX = rightEdge.X - interiorRect.X;
        if (filledWidth > rightEdgeStartX)
            primitiveRenderer.DrawRectangle(spriteBatch, rightEdge, fillColor, FillLayerDepth);
    }

    private void DrawCorners(SpriteBatch spriteBatch, Rectangle interiorRect, int filledWidth)
    {
        List<(Vector2 position, float rotation)> toDraw = [];

        if (filledWidth > 0)
            toDraw.AddRange([Frame.TopLeftTriangle, Frame.BottomLeftTriangle]);

        if (filledWidth >= interiorRect.X)
            toDraw.AddRange([Frame.TopRightTriangle, Frame.BottomRightTriangle]);

        foreach (var (pos, rotation) in toDraw)
            primitiveRenderer.DrawTriangle(spriteBatch, pos, fillColor, rotation, FillLayerDepth);
    }
}