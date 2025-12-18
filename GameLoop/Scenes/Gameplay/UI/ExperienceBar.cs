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
        var panel = panelRenderer.Define(centre, interiorSize);
        return new ExperienceBar(panel, primitiveRenderer, levelManager);
    }
}

internal class ExperienceBar(Panel panel, PrimitiveRenderer primitiveRenderer, LevelManager levelManager)
{
    private float Progress => MathHelper.Clamp(
        levelManager.ExperienceSinceLastLevel / levelManager.ExperienceToNextLevel,
        0f, 1f);

    /// <summary>
    ///     Draws an experience bar with a filled portion according to <paramref name="progress" /> (0..1).
    /// </summary>
    internal void Draw(
        SpriteBatch spriteBatch,
        Color frameColor,
        Color fillColor,
        float layerDepth = 0f)
    {
        spriteBatch.Begin(SpriteSortMode.FrontToBack);
        var frame = panel.Frame;

        // --- 1. Compute filled width ---
        var interiorRect = frame.InteriorRectangle;
        var filledWidth = (int)(interiorRect.Width * Progress);

        if (filledWidth <= 0)
        {
            // Nothing to fill, just draw the frame
            panel.Draw(spriteBatch, frameColor, Color.Transparent);
            spriteBatch.End();
            return;
        }

        // --- 2. Fill the center rectangle ---
        var filledCenterRect = new Rectangle(interiorRect.X, interiorRect.Y, filledWidth, interiorRect.Height);
        primitiveRenderer.DrawRectangle(spriteBatch, filledCenterRect, fillColor, layerDepth);

        var topEdge = frame.TopEdgeRectangle;
        var topInterior = new Rectangle(topEdge.X, topEdge.Y, Math.Min((int)(topEdge.Width * Progress), filledWidth),
            topEdge.Height);

        var bottomEdge = frame.BottomEdgeRectangle;
        var bottomInterior = new Rectangle(bottomEdge.X, bottomEdge.Y,
            Math.Min((int)(bottomEdge.Width * Progress), filledWidth), bottomEdge.Height);

        var leftEdge = frame.LeftEdgeRectangle;

        IEnumerable<Rectangle> toDraw = [topInterior, bottomInterior, leftEdge];
        foreach (var rectangle in toDraw)
            primitiveRenderer.DrawRectangle(spriteBatch, rectangle, fillColor, layerDepth);

        // Only draw right edge once the fill reaches it
        var rightEdge = frame.RightEdgeRectangle;
        var rightEdgeStartX = rightEdge.X - interiorRect.X;
        if (filledWidth > rightEdgeStartX)
            primitiveRenderer.DrawRectangle(
                spriteBatch,
                rightEdge,
                fillColor,
                layerDepth);

        // Only draw the triangle if it lies within the filled width
        var trianglesWithinFilledWidth = frame.CornerTriangles.Where(t => t.topLeft.X - interiorRect.X < filledWidth);
        foreach (var (pos, rotation) in trianglesWithinFilledWidth)
            primitiveRenderer.DrawTriangle(spriteBatch, pos, fillColor, rotation, layerDepth);

        panel.Draw(spriteBatch, frameColor, Color.Transparent);
        spriteBatch.End();
    }
}