using GameLoop.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Veil.Desktop.PlatformServices.Rendering;

public sealed class DesktopViewportSync : IViewportSync
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly RenderScaler _renderScaler;

    private int _lastW;
    private int _lastH;

    public DesktopViewportSync(GraphicsDevice graphicsDevice, RenderScaler renderScaler)
    {
        _graphicsDevice = graphicsDevice;
        _renderScaler = renderScaler;
        ForceRefresh();
    }

    public void ForceRefresh()
    {
        _lastW = 0;
        _lastH = 0;
        Update();
    }

    public void Update()
    {
        var pp = _graphicsDevice.PresentationParameters;
        var w = pp.BackBufferWidth;
        var h = pp.BackBufferHeight;

        if (w == _lastW && h == _lastH)
            return;

        _lastW = w;
        _lastH = h;

        _renderScaler.UpdateOutputRectangle();
    }
}