using GameLoop.Rendering;
using Microsoft.JSInterop;
using Microsoft.Xna.Framework;

namespace Veil.Web.PlatformServices.Rendering;

public sealed class WebViewportSync(GraphicsDeviceManager graphics, RenderScaler renderScaler, IJSInProcessRuntime js)
    : IViewportSync
{
    private int _lastBackBufferWidth;
    private int _lastBackBufferHeight;

    public void ForceRefresh()
    {
        _lastBackBufferWidth = 0;
        _lastBackBufferHeight = 0;
        Update();
    }

    public void Update()
    {
        var m = js.Invoke<RenderMetrics>("veilGraphics.getRenderMetrics");
        if (m.BackBufferWidth <= 0 || m.BackBufferHeight <= 0)
            return;

        if (m.BackBufferWidth == _lastBackBufferWidth && m.BackBufferHeight == _lastBackBufferHeight)
            return;

        graphics.PreferredBackBufferWidth = m.BackBufferWidth;
        graphics.PreferredBackBufferHeight = m.BackBufferHeight;
        graphics.ApplyChanges();

        _lastBackBufferWidth = m.BackBufferWidth;
        _lastBackBufferHeight = m.BackBufferHeight;

        renderScaler.UpdateOutputRectangle();
    }

    private readonly record struct RenderMetrics(
        float CssWidth,
        float CssHeight,
        float Dpr,
        int BackBufferWidth,
        int BackBufferHeight);
}