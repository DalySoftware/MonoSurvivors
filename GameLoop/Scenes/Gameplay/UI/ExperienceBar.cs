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
    PrimitiveRenderer primitiveRenderer,
    LevelManager levelManager,
    SphereGrid sphereGrid,
    GameInputState inputState,
    Panel.Factory panelFactory)
{
    internal ExperienceBar Create()
    {
        var font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Medium);

        const float interiorHeight = 20f;
        const float padding = 50f;

        // Main bar
        var interiorSize = new Vector2(viewport.Width * 0.7f, interiorHeight);
        var barSize = Panel.Factory.MeasureByInterior(interiorSize);

        var barRect = viewport
            .UiRectangle()
            .CreateAnchoredRectangle(UiAnchor.BottomCenter, barSize, new Vector2(0f, -padding));

        var barPanel = panelFactory.DefineByExterior(barRect);

        var progressBar = new PanelProgressBar(
            barPanel,
            primitiveRenderer,
            ColorPalette.Agave,
            Color.SlateGray,
            ColorPalette.Green
        );

        // Points box
        var pointsInteriorSize = new Vector2(interiorHeight, interiorHeight);
        var pointsBoxSize = Panel.Factory.MeasureByInterior(pointsInteriorSize);

        var pointsBoxRect = barRect.CreateAnchoredRectangle(UiAnchor.CenterRight, pointsBoxSize);
        var pointsBox = panelFactory.DefineByExterior(pointsBoxRect, barPanel.Frame.LayerDepth + 0.05f);

        return new ExperienceBar(
            font,
            progressBar,
            pointsBox,
            levelManager,
            sphereGrid,
            inputState
        );
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
        levelManager.ExperienceSinceLastLevel / levelManager.ExperienceToNextLevel, 0f, 1f);

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

        var button = inputState.CurrentInputMethod is InputMethod.KeyboardMouse ? "SPACE" : "[Y]";
        var spendPrompt = $"Press {button} to spend your points!";
        var textSize = font.MeasureString(spendPrompt);

        // Centre the text horizontally in the bar
        var interiorRect = progressBar.InteriorRectangle;
        var textPosition = interiorRect.TopLeft() + (interiorRect.Size.ToVector2() - textSize) * 0.5f;

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

        // Anchor text to the panel center
        var textOrigin = textSize * 0.5f; // center of the text
        var textPosition = pointsBoxPanel.Interior.AnchorForPoint(UiAnchor.Centre);
        var textLayer = (pointsBoxPanel.InteriorLayerDepth + pointsBoxPanel.Frame.LayerDepth) * 0.5f;

        spriteBatch.DrawString(font, text, textPosition, origin: textOrigin, scale: Vector2.One * textScale,
            layerDepth: textLayer
        );
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