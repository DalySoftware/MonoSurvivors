using System;
using ContentLibrary;
using Gameplay.Entities.Enemies.Spawning;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay.UI;

internal class RunClockFactory(ContentManager content, Viewport viewport, EnemySpawner spawner)
{
    internal RunClock Create()
    {
        const float padding = 50f;
        var font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);

        var size = RunClock.Measure(font);
        var middleRight = new Vector2(viewport.Width - padding, padding + size.Y * 0.5f);

        return new RunClock(font, spawner, middleRight);
    }
}

internal class RunClock(SpriteFont font, EnemySpawner spawner, Vector2 middleRight)
{
    internal void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        var text = Format(spawner.ElapsedTime);
        var size = font.MeasureString(text);
        var origin = new Vector2(size.X, font.LineSpacing * .67f);

        spriteBatch.DrawString(font, text, middleRight, ColorPalette.White, origin: origin);
        spriteBatch.End();
    }

    internal static Vector2 Measure(SpriteFont font) => font.MeasureString(Format(TimeSpan.FromMinutes(30)));

    private static string Format(TimeSpan timespan) => timespan.ToString("mm\\:ss");
}