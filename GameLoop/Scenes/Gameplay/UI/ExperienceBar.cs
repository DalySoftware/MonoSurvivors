using System;
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

        const float interiorHeight = 20f;
        const float padding = 50f;
        var centre = new Vector2(viewport.Bounds.Center.ToVector2().X,
            viewport.Bounds.Height - interiorHeight - padding);
        var width = viewport.Width * 0.7f;
        var interiorSize = new Vector2(width, interiorHeight);
        var barPanel = panelRenderer.Define(centre, interiorSize, Layers.Ui + 0.05f);

        var progressBar = new PanelProgressBar(barPanel, primitiveRenderer, ColorPalette.Agave, Color.SlateGray,
            ColorPalette.Green);
        var pointsBoxSize = new Vector2(interiorSize.Y, interiorSize.Y);
        var pointsBox =
            panelRenderer.Define(barPanel.Frame.MiddleRight, pointsBoxSize, barPanel.Frame.LayerDepth + 0.05f);

        return new ExperienceBar(font, progressBar, pointsBox, levelManager, sphereGrid, inputState);
    }
}

internal class ExperienceBar(
    SpriteFont font,
    PanelProgressBar progressBar,
    Panel pointsBoxPanel,
    LevelManager levelManager,
    SphereGrid sphereGrid,
    GameInputState inputState)
{
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
        _provideFeedbackToSpendPoints = sphereGrid.AvailablePoints >= 3 ||
                                        (sphereGrid.AvailablePoints >= 1 &&
                                         gameTime.ElapsedGameTime <= TimeSpan.FromMinutes(3));

        progressBar.Progress = Progress;

        spriteBatch.Begin(SpriteSortMode.FrontToBack);

        progressBar.Draw(spriteBatch);
        DrawPointsBox(spriteBatch, gameTime);
        DrawSpendPrompt(spriteBatch);

        spriteBatch.End();
    }

    private void DrawSpendPrompt(SpriteBatch spriteBatch)
    {
        // Draw feedback text if applicable
        if (!_provideFeedbackToSpendPoints) return;

        var button = inputState.CurrentInputMethod is InputMethod.KeyboardMouse ? "SPACE" : "[Back]";
        var spendPrompt = $"Press {button} to spend your points!";
        var textSize = font.MeasureString(spendPrompt);

        // Centre the text horizontally in the bar
        var interiorRect = progressBar.InteriorRectangle;
        var textPosition = new Vector2(
            interiorRect.X + interiorRect.Width * 0.5f - textSize.X * 0.5f,
            interiorRect.Y + interiorRect.Height * 0.5f - textSize.Y * 0.5f
        );

        // Draw slightly above the bar fill layer
        var textLayer = progressBar.FillLayerDepth + 0.01f;
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
}