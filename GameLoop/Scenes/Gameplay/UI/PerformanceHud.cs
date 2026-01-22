using ContentLibrary;
using GameLoop.Debug;
using GameLoop.Rendering;
using GameLoop.UI;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay.UI;

internal sealed class PerformanceHud(SpriteFont font, RenderScaler viewport, PerformanceMetrics metrics)
{
    private const double TextRefreshSeconds = 0.25;

    private bool _visible = false;

    private string _text = "";
    private Vector2 _textSize;
    private double _timeSinceRefresh;

    private UiRectangle _uiRectangle;

    public void Toggle() => _visible = !_visible;

    public void Update(GameTime gameTime)
    {
        if (!_visible) return;

        _timeSinceRefresh += gameTime.ElapsedGameTime.TotalSeconds;
        if (_timeSinceRefresh < TextRefreshSeconds) return;
        _timeSinceRefresh = 0;

        var w = viewport.Width;
        var h = viewport.Height;

        _text =
            $"FPS {metrics.Fps,3} | U {metrics.UpdateMs,6:0.00}ms | D {metrics.DrawMs,6:0.00}ms" +
            $" | GC {metrics.Gc0Delta}/{metrics.Gc1Delta}/{metrics.Gc2Delta} | {w}x{h}";

        _textSize = font.MeasureString(_text);
        _uiRectangle = viewport.UiRectangle()
            .CreateAnchoredRectangle(UiAnchor.BottomCenter, _textSize, new Vector2(0f, -150f));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!_visible) return;

        // Compute middle-right anchor each draw (handles resizing)
        spriteBatch.Begin();
        spriteBatch.DrawString(font, _text, _uiRectangle.TopLeft, ColorPalette.LightGray);
        spriteBatch.End();
    }
}

internal sealed class PerformanceHudFactory(
    ContentManager content,
    RenderScaler viewport,
    PerformanceMetrics metrics)
{
    internal PerformanceHud Create()
    {
        // Pick the font you want; using your existing path pattern
        var font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);
        return new PerformanceHud(font, viewport, metrics);
    }
}