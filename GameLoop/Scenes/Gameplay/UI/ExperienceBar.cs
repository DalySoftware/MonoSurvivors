using System;
using System.Collections.Generic;
using ContentLibrary;
using GameLoop.Input;
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
    SphereGrid sphereGrid,
    GameInputState inputState
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
        return new ExperienceBar(font, barPanel, pointsBox, primitiveRenderer, levelManager, sphereGrid, inputState);
    }
}

internal class ExperienceBar(
    SpriteFont font,
    Panel barPanel,
    Panel pointsBoxPanel,
    PrimitiveRenderer primitiveRenderer,
    LevelManager levelManager,
    SphereGrid sphereGrid,
    GameInputState inputState)
{
    internal const float InteriorHeight = 20f;

    private readonly float _fillLayerDepth = (barPanel.InteriorLayerDepth + barPanel.Frame.LayerDepth) * 0.5f;
    private bool _provideFeedbackToSpendPoints;
    private int Points => sphereGrid.AvailablePoints;

    private float Progress => MathHelper.Clamp(
        levelManager.ExperienceSinceLastLevel / levelManager.ExperienceToNextLevel,
        0f, 1f);

    /// <summary>
    ///     Draws an experience bar with a filled portion according to <paramref name="progress" /> (0..1).
    /// </summary>
    internal void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        _provideFeedbackToSpendPoints =
            Points >= 3 || (Points >= 1 && gameTime.ElapsedGameTime <= TimeSpan.FromMinutes(3));

        var frameColor = ColorPalette.Agave;
        var fillColor = ColorPalette.Green;

        spriteBatch.Begin(SpriteSortMode.FrontToBack);

        var frame = barPanel.Frame;
        var interiorRect = frame.InteriorRectangle;
        var filledWidth = (int)(interiorRect.Width * Progress);

        DrawInterior(spriteBatch, fillColor, interiorRect, filledWidth);
        DrawEdges(spriteBatch, fillColor, frame, filledWidth, interiorRect);
        DrawCorners(spriteBatch, fillColor, frame, filledWidth, interiorRect);

        // Draw the bar panel itself (background + frame)
        barPanel.Draw(spriteBatch, frameColor, Color.SlateGray);

        // Draw points box
        DrawPointsBox(spriteBatch, gameTime);

        DrawSpendPrompt(spriteBatch, interiorRect);

        spriteBatch.End();
    }

    private void DrawSpendPrompt(SpriteBatch spriteBatch, Rectangle interiorRect)
    {
        // Draw feedback text if applicable
        if (!_provideFeedbackToSpendPoints) return;

        var button = inputState.CurrentInputMethod is InputMethod.KeyboardMouse ? "SPACE" : "[Back]";
        var spendPrompt = $"Press {button} to spend your points!";
        var textSize = font.MeasureString(spendPrompt);

        // Centre the text horizontally in the bar
        var textPosition = new Vector2(
            interiorRect.X + interiorRect.Width * 0.5f - textSize.X * 0.5f,
            interiorRect.Y + interiorRect.Height * 0.5f - textSize.Y * 0.5f
        );

        // Draw slightly above the bar fill layer
        var textLayer = _fillLayerDepth + 0.01f;
        spriteBatch.DrawString(font, spendPrompt, textPosition, Color.White, layerDepth: textLayer);
    }

    private void DrawPointsBox(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var boxColor = PointsBoxTint(gameTime);

        pointsBoxPanel.Draw(spriteBatch, boxColor, boxColor.ShiftChroma(-0.08f));

        var text = Points.ToString();
        var textSize = font.MeasureString(text);
        var textScale = PointsTextScale(gameTime);
        var scaledSize = textSize * textScale;

        var panelCentre = pointsBoxPanel.Centre;
        var textPosition = panelCentre - scaledSize * 0.5f;

        var textLayer = (pointsBoxPanel.InteriorLayerDepth + pointsBoxPanel.Frame.LayerDepth) * 0.5f;
        spriteBatch.DrawString(font, text, textPosition, Color.White, layerDepth: textLayer,
            scale: Vector2.One * textScale);
    }

    private Color PointsBoxTint(GameTime gameTime)
    {
        var baseColor = Color.SlateBlue;
        if (!_provideFeedbackToSpendPoints) return baseColor;

        var pulse = 0.5f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2);
        return Color.Lerp(baseColor, Color.Gold, pulse);
    }

    private float PointsTextScale(GameTime gameTime)
    {
        if (!_provideFeedbackToSpendPoints) return 1f;

        return 1.3f + 0.1f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2);
    }

    private void DrawCorners(SpriteBatch spriteBatch, Color fillColor, Frame frame, int filledWidth,
        Rectangle interiorRect)
    {
        List<(Vector2 position, float rotation)> toDraw = [];

        if (filledWidth > 0)
            toDraw.AddRange([frame.TopLeftTriangle, frame.BottomLeftTriangle]);

        if (filledWidth >= interiorRect.X)
            toDraw.AddRange([frame.TopRightTriangle, frame.BottomRightTriangle]);

        foreach (var (pos, rotation) in toDraw)
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

        IEnumerable<Rectangle> toDraw = [topInterior, bottomInterior];
        foreach (var rectangle in toDraw)
            primitiveRenderer.DrawRectangle(spriteBatch, rectangle, fillColor, _fillLayerDepth);

        if (filledWidth > 0)
        {
            var leftEdge = frame.LeftEdgeRectangle;
            primitiveRenderer.DrawRectangle(spriteBatch, leftEdge, fillColor, _fillLayerDepth);
        }

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