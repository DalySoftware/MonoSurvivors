using System;
using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using GameLoop.UI;
using Gameplay.Levelling;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay.UI;

internal sealed class ExperienceBarFactory(
    ContentManager content,
    Viewport viewport,
    PanelRenderer panelRenderer,
    PrimitiveRenderer primitiveRenderer,
    LevelManager levelManager,
    SphereGrid sphereGrid
)
{
    internal ExperienceBar Create()
    {
        var font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Medium);

        const float padding = 50f;
        var centre = new Vector2(viewport.Bounds.Center.ToVector2().X,
            viewport.Bounds.Height - ExperienceBar.InteriorHeight - padding);
        var width = viewport.Width * 0.7f;
        var interiorSize = new Vector2(width, ExperienceBar.InteriorHeight);

        var barPanel = panelRenderer.Define(centre, interiorSize, Layers.Ui + 0.05f);
        var pointsBoxSize = new Vector2(interiorSize.Y, interiorSize.Y);
        var pointsBox =
            panelRenderer.Define(barPanel.Frame.MiddleRight, pointsBoxSize, barPanel.Frame.LayerDepth + 0.05f);
        return new ExperienceBar(font, barPanel, pointsBox, primitiveRenderer, levelManager, sphereGrid);
    }
}

internal class ExperienceBar(
    SpriteFont font,
    Panel barPanel,
    Panel pointsBoxPanel,
    PrimitiveRenderer primitiveRenderer,
    LevelManager levelManager,
    SphereGrid sphereGrid)
{
    internal const float InteriorHeight = 20f;

    private readonly float _fillLayerDepth = (barPanel.InteriorLayerDepth + barPanel.Frame.LayerDepth) * 0.5f;
    private int Points => sphereGrid.AvailablePoints;

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
        var frame = barPanel.Frame;

        var interiorRect = frame.InteriorRectangle;
        var filledWidth = (int)(interiorRect.Width * Progress);

        DrawInterior(spriteBatch, fillColor, interiorRect, filledWidth);
        DrawEdges(spriteBatch, fillColor, frame, filledWidth, interiorRect);
        DrawCorners(spriteBatch, fillColor, frame, interiorRect, filledWidth);

        // We set layer depths to ensure our filled parts are above the background but below the frame
        barPanel.Draw(spriteBatch, frameColor, Color.SlateGray);

        DrawPointsBox(spriteBatch);


        spriteBatch.End();
    }
    private void DrawPointsBox(SpriteBatch spriteBatch)
    {
        var boxColor = Color.SlateBlue;
        pointsBoxPanel.Draw(spriteBatch, boxColor, boxColor.ShiftChroma(-0.08f));

        var text = Points.ToString();
        var textSize = font.MeasureString(text);

        var panelCentre = pointsBoxPanel.Centre;
        var textPosition = panelCentre - textSize * 0.5f;

        var textLayer = (pointsBoxPanel.InteriorLayerDepth + pointsBoxPanel.Frame.LayerDepth) * 0.5f;
        spriteBatch.DrawString(font, text, textPosition, Color.White, layerDepth: textLayer);
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