using System;
using ContentLibrary;
using Gameplay.Entities.Enemies.Spawning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay.UI;

internal class RunClockFactory(ContentManager content, Viewport viewport, EnemySpawner spawner)
{
    internal RunClock Create()
    {
        var font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);
        var size = RunClock.Measure(font);
        var x = viewport.Width - size.X - 10f;
        var y = 10f;
        var topLeft = new Vector2(x, y);

        return new RunClock(font, spawner, topLeft);
    }
}

internal class RunClock(SpriteFont font, EnemySpawner spawner, Vector2 topLeft)
{
    internal void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        var text = Format(spawner.ElapsedTime);
        spriteBatch.DrawString(font, text, topLeft, Color.White);
        spriteBatch.End();
    }

    internal static Vector2 Measure(SpriteFont font) => font.MeasureString(Format(TimeSpan.FromMinutes(30)));

    private static string Format(TimeSpan timespan) => timespan.ToString("mm\\:ss");
}