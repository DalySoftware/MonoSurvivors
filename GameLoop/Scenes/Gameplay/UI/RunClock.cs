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
    private int _lastSecond = -1;
    private string _cached = "00:00";
    internal void Draw(SpriteBatch spriteBatch)
    {
        var text = Format(spawner.ElapsedTime);
        spriteBatch.DrawString(font, text, rectangle.TopLeft, ColorPalette.White);
    }

    internal static Vector2 Measure(SpriteFont font) => font.MeasureString("00:00");

    private string Format(TimeSpan t)
    {
        var s = (int)t.TotalSeconds;
        if (s == _lastSecond) return _cached;
        _lastSecond = s;

        var mm = s / 60 % 60;
        var ss = s % 60;
        _cached = $"{mm:00}:{ss:00}";
        return _cached;
    }
}