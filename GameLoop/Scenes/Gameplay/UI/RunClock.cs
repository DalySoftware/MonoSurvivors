using System;
using ContentLibrary;
using GameLoop.Rendering;
using GameLoop.UI;
using Gameplay.Entities.Enemies.Spawning;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay.UI;

internal class RunClockFactory(ContentManager content, RenderScaler viewport, EnemySpawner spawner)
{
    internal RunClock Create()
    {
        const float padding = 50f;
        var font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);

        var size = RunClock.Measure(font);
        var rectangle = viewport.UiRectangle()
            .CreateAnchoredRectangle(UiAnchor.TopRight, size, new Vector2(-padding, padding));

        return new RunClock(font, spawner, rectangle);
    }
}

internal class RunClock(SpriteFont font, EnemySpawner spawner, UiRectangle rectangle)
{
    internal void Draw(SpriteBatch spriteBatch)
    {
        var text = Format(spawner.ElapsedTime);
        spriteBatch.DrawString(font, text, rectangle.TopLeft, ColorPalette.White);
    }

    internal static Vector2 Measure(SpriteFont font) => font.MeasureString(Format(TimeSpan.FromMinutes(30)));

    private static string Format(TimeSpan timespan) => timespan.ToString("mm\\:ss");
}