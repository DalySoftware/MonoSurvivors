using System.Linq;
using ContentLibrary;
using GameLoop.Rendering;
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
    RenderScaler renderScaler,
    PrimitiveRenderer primitiveRenderer,
    EnemySpawner enemySpawner,
    Panel.Factory panelFactory)
{
    internal BossHealthBar Create()
    {
        var font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);

        const float interiorHeight = 20f;
        var interiorSize = new Vector2(renderScaler.Width * 0.6f, interiorHeight);

        // Root rectangle = full viewport
        var panelSize = Panel.Factory.MeasureByInterior(interiorSize);

        const float topPadding = 50f;
        var panelRect = renderScaler
            .UiRectangle()
            .CreateAnchoredRectangle(UiAnchor.TopCenter, panelSize, new Vector2(0f, topPadding));

        var panel = panelFactory.DefineByExterior(panelRect, Layers.Ui + 0.05f);

        var progressBar = new PanelProgressBar(
            panel,
            primitiveRenderer,
            ColorPalette.Violet,
            ColorPalette.LightGray,
            ColorPalette.Red
        );

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

        var interiorRect = progressBar.InteriorRectangle;
        var textPosition = interiorRect.TopLeft() + (interiorRect.Size.ToVector2() - textSize) * 0.5f;

        var textLayer = progressBar.FillLayerDepth + 0.01f;
        spriteBatch.DrawString(font, bossName, textPosition, ColorPalette.White, layerDepth: textLayer);
    }

    private EnemyBase? GetActiveBoss() => enemySpawner.ActiveBosses.FirstOrDefault();
}