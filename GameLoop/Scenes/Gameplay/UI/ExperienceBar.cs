using System;
using ContentLibrary;
using GameLoop.Input;
using GameLoop.Rendering;
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
    RenderScaler renderScaler,
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
        var interiorSize = new Vector2(renderScaler.Width * 0.7f, interiorHeight);
        var barSize = Panel.Factory.MeasureByInterior(interiorSize);

        var barRect = renderScaler
            .UiRectangle()
            .CreateAnchoredRectangle(UiAnchor.BottomCenter, barSize, new Vector2(0f, -padding));

        var barPanel = panelFactory.DefineByExterior(barRect);

        var progressBar = new PanelProgressBar(barPanel, primitiveRenderer, ColorPalette.Agave, ColorPalette.DarkGray,
            ColorPalette.Lime);

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
    private readonly static TimeSpan SiphonDuration = TimeSpan.FromSeconds(0.5);
    private readonly static TimeSpan ReceivePulseDuration = TimeSpan.FromSeconds(0.7);

    private bool _provideFeedbackToSpendPoints;

    private readonly ExperiencePointsSyphon _pointsSyphon =
        new(SiphonDuration, ReceivePulseDuration, sphereGrid.AvailablePoints);

    private Color _pointsBoxFrameColor;
    private Color _pointsBoxInteriorColor;
    private float _pointsTextScale;

    private bool _showSpendPrompt;
    private InputMethod _lastPromptInputMethod;
    private string _spendPrompt = string.Empty;

    private int ActualPoints => sphereGrid.AvailablePoints;

    private float Progress => MathHelper.Clamp(
        levelManager.ExperienceSinceLastLevel / levelManager.ExperienceToNextLevel, 0f, 1f);

    internal void Update(GameTime gameTime)
    {
        _provideFeedbackToSpendPoints = sphereGrid.AvailablePoints >= 3 ||
                                        (sphereGrid.AvailablePoints >= 1 &&
                                         gameTime.TotalGameTime <= TimeSpan.FromMinutes(3));

        _pointsSyphon.Update(ActualPoints, gameTime.ElapsedGameTime);

        var draining = _pointsSyphon.IsDraining;
        progressBar.Start = draining ? _pointsSyphon.DrainStartFraction : 0f;
        progressBar.End = draining ? 1f : Progress;

        _pointsBoxFrameColor = PointsBoxTint(gameTime);
        _pointsBoxInteriorColor = _pointsBoxFrameColor.ShiftChroma(-0.08f);
        _pointsTextScale = PointsTextScale(gameTime);

        _showSpendPrompt = _provideFeedbackToSpendPoints;
        UpdateSpendPromptText();
    }

    private void UpdateSpendPromptText()
    {
        if (!_showSpendPrompt)
        {
            _spendPrompt = string.Empty;
            return;
        }

        var inputMethod = inputState.CurrentInputMethod;
        if (_spendPrompt.Length > 0 && inputMethod == _lastPromptInputMethod)
            return;

        _lastPromptInputMethod = inputMethod;

        var button = inputMethod is InputMethod.KeyboardMouse ? "SPACE" : "[Y]";
        _spendPrompt = $"Press {button} to spend your points!";
    }

    internal void Draw(SpriteBatch spriteBatch)
    {
        progressBar.Draw(spriteBatch);
        DrawPointsBox(spriteBatch);
        DrawSpendPrompt(spriteBatch);
    }

    private void DrawSpendPrompt(SpriteBatch spriteBatch)
    {
        if (!_showSpendPrompt) return;

        var textSize = font.MeasureString(_spendPrompt);

        var interiorRect = progressBar.InteriorRectangle;
        var textPosition = interiorRect.TopLeft() + (interiorRect.Size.ToVector2() - textSize) * 0.5f;

        var textLayer = progressBar.FillLayerDepth + 0.01f;
        spriteBatch.DrawString(font, _spendPrompt, textPosition, ColorPalette.Candy, layerDepth: textLayer);
    }

    private void DrawPointsBox(SpriteBatch spriteBatch)
    {
        pointsBoxPanel.Draw(spriteBatch, _pointsBoxFrameColor, _pointsBoxInteriorColor);

        var text = _pointsSyphon.DisplayPoints.ToString();
        var textSize = font.MeasureString(text);

        var textOrigin = textSize * 0.5f;
        var textPosition = pointsBoxPanel.Interior.AnchorForPoint(UiAnchor.Centre);
        var textLayer = (pointsBoxPanel.InteriorLayerDepth + pointsBoxPanel.Frame.LayerDepth) * 0.5f;

        spriteBatch.DrawString(
            font,
            text,
            textPosition,
            origin: textOrigin,
            scale: Vector2.One * _pointsTextScale,
            layerDepth: textLayer,
            color: ColorPalette.LightGray);
    }

    private Color PointsBoxTint(GameTime gameTime)
    {
        var color = ColorPalette.Royal;

        // "spend points" hint pulse, 0..1
        if (_provideFeedbackToSpendPoints)
        {
            var t = 0.5f + 0.5f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2);
            color = Color.Lerp(color, ColorPalette.Lime, 0.35f * t);
        }

        // While siphoning, add a "charging" bias towards Lime.
        if (_pointsSyphon.IsDraining)
            color = Color.Lerp(color, ColorPalette.Lime, 0.25f * _pointsSyphon.ChargeStrength);

        // After siphon completes, add a stronger "received" pulse.
        var receivePulse = _pointsSyphon.ReceivePulseStrength;
        if (receivePulse > 0f)
        {
            var p = receivePulse * receivePulse; // ease-out
            color = Color.Lerp(color, ColorPalette.Candy, 0.65f * p);
        }

        return color;
    }

    private float PointsTextScale(GameTime gameTime)
    {
        var scale = 1f;

        if (_provideFeedbackToSpendPoints)
            scale = 1.3f + 0.1f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2);

        if (_pointsSyphon.IsDraining)
            scale += 0.05f * _pointsSyphon.ChargeStrength;

        var receivePulse = _pointsSyphon.ReceivePulseStrength;
        if (receivePulse > 0f)
        {
            var p = receivePulse * receivePulse;
            scale += 0.25f * p;
        }

        return scale;
    }
}