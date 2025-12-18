using System;
using System.Collections.Generic;
using System.Linq;
using GameLoop.UI;
using Gameplay.Levelling;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay.UI;

internal sealed class ExperienceBarRenderer(
    PanelRenderer panelRenderer,
    PrimitiveRenderer primitiveRenderer,
    LevelManager levelManager)
{
    internal ExperienceBar Define(Vector2 centre, Vector2 interiorSize)
    {
        var panel = panelRenderer.Define(centre, interiorSize, Layers.Ui + 0.05f);
        return new ExperienceBar(panel, primitiveRenderer, levelManager, sphereGrid);
    }
}

internal class ExperienceBar(Panel panel, PrimitiveRenderer primitiveRenderer, LevelManager levelManager)
{
    private readonly float _fillLayerDepth = (panel.InteriorLayerDepth + panel.Frame.LayerDepth) * 0.5f;

    private float Progress => MathHelper.Clamp(
        levelManager.ExperienceSinceLastLevel / levelManager.ExperienceToNextLevel,
        0f, 1f);

    /// <summary>
    ///     Draws an experience bar with a filled portion according to <paramref name="progress" /> (0..1).
    /// </summary>
    internal void Draw(
        SpriteBatch spriteBatch,
        Color frameColor,
        Color fillColor)
    {
        spriteBatch.Begin(SpriteSortMode.FrontToBack);
        var frame = panel.Frame;

        // --- 1. Compute filled width ---
        var interiorRect = frame.InteriorRectangle;
        var filledWidth = (int)(interiorRect.Width * Progress);

        if (filledWidth <= 0)
        {
            // Nothing to fill, just draw the frame
            panel.Draw(spriteBatch, frameColor, Color.SlateGray);
            spriteBatch.End();
            return;
        }

        DrawInterior(spriteBatch, fillColor, interiorRect, filledWidth);
        DrawEdges(spriteBatch, fillColor, frame, filledWidth, interiorRect);
        DrawCorners(spriteBatch, fillColor, frame, interiorRect, filledWidth);

        // We set layer depths to ensure our filled parts are above the background but below the frame
        panel.Draw(spriteBatch, frameColor, Color.SlateGray);
        spriteBatch.End();
    }

    private void DrawCorners(SpriteBatch spriteBatch, Color fillColor, Frame frame, Rectangle interiorRect,
        int filledWidth)
    {
        // Only draw the triangle if it lies within the filled width
        var trianglesWithinFilledWidth = frame.CornerTriangles.Where(t => t.topLeft.X - interiorRect.X < filledWidth);
        foreach (var (pos, rotation) in trianglesWithinFilledWidth)
            primitiveRenderer.DrawTriangle(spriteBatch, pos, fillColor, rotation, _fillLayerDepth);
    }
    private void DrawEdges(SpriteBatch spriteBatch, Color fillColor, Frame frame, int filledWidth,
        Rectangle interiorRect)
    {
        var topEdge = frame.TopEdgeRectangle;
        var topInterior = new Rectangle(topEdge.X, topEdge.Y, Math.Min((int)(topEdge.Width * Progress), filledWidth),
            topEdge.Height);

        var bottomEdge = frame.BottomEdgeRectangle;
        var bottomInterior = new Rectangle(bottomEdge.X, bottomEdge.Y,
            Math.Min((int)(bottomEdge.Width * Progress), filledWidth), bottomEdge.Height);

        var leftEdge = frame.LeftEdgeRectangle;

        IEnumerable<Rectangle> toDraw = [topInterior, bottomInterior, leftEdge];
        foreach (var rectangle in toDraw)
            primitiveRenderer.DrawRectangle(spriteBatch, rectangle, fillColor, _fillLayerDepth);

        // Only draw right edge once the fill reaches it
        var rightEdge = frame.RightEdgeRectangle;
        var rightEdgeStartX = rightEdge.X - interiorRect.X;
        if (filledWidth > rightEdgeStartX)
            primitiveRenderer.DrawRectangle(
                spriteBatch,
                rightEdge,
                fillColor, _fillLayerDepth);
    }
    private void DrawInterior(SpriteBatch spriteBatch, Color fillColor, Rectangle interiorRect, int filledWidth)
    {
        var filledCenterRect = new Rectangle(interiorRect.X, interiorRect.Y, filledWidth, interiorRect.Height);
        primitiveRenderer.DrawRectangle(spriteBatch, filledCenterRect, fillColor, _fillLayerDepth);
    }
}