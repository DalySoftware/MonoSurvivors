using System.Linq;
using ContentLibrary;
using GameLoop.UI;
using Gameplay.Entities.Enemies;
using Gameplay.Entities.Enemies.Spawning;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay.UI;

internal sealed class BossHealthBarFactory(
    ContentManager content,
    Viewport viewport,
    PanelRenderer panelRenderer,
    PrimitiveRenderer primitiveRenderer,
    EnemySpawner enemySpawner)
{
    internal BossHealthBar Create()
    {
        var font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);

        const float interiorHeight = 20f;
        const float padding = 50f;
        var centre = new Vector2(viewport.Bounds.Center.ToVector2().X, interiorHeight + padding);

        var size = new Vector2(viewport.Width * 0.6f, interiorHeight);
        var panel = panelRenderer.Define(centre, size, Layers.Ui + 0.05f);

        var progressBar = new PanelProgressBar(panel, primitiveRenderer, ColorPalette.Violet, ColorPalette.LightGray,
            ColorPalette.Red);
        return new BossHealthBar(progressBar, enemySpawner, font);
    }
}

internal sealed class BossHealthBar(
    PanelProgressBar progressBar,
    EnemySpawner enemySpawner,
    SpriteFont font)
{
    internal void Draw(SpriteBatch spriteBatch)
    {
        var boss = GetActiveBoss();
        if (boss is null)
            return;

        // Update the progress
        progressBar.Progress = boss.Health / boss.Stats.MaxHealth;

        spriteBatch.Begin(SpriteSortMode.FrontToBack);

        progressBar.Draw(spriteBatch);
        DrawBossName(spriteBatch);

        spriteBatch.End();
    }

    private void DrawBossName(SpriteBatch spriteBatch)
    {
        const string bossName = "JORGIE";
        var textSize = font.MeasureString(bossName);

        // Centre the text horizontally in the bar
        var interiorRect = progressBar.InteriorRectangle;
        var textPosition = new Vector2(
            interiorRect.X + interiorRect.Width * 0.5f - textSize.X * 0.5f,
            interiorRect.Y + interiorRect.Height * 0.5f - textSize.Y * 0.5f
        );

        // Draw slightly above the bar fill layer
        var textLayer = progressBar.FillLayerDepth + 0.01f;
        spriteBatch.DrawString(font, bossName, textPosition, ColorPalette.White, layerDepth: textLayer);
    }

    private EnemyBase? GetActiveBoss() => enemySpawner.ActiveBosses.FirstOrDefault();
}