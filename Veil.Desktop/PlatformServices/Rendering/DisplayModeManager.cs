using GameLoop.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Veil.Desktop.PlatformServices.Rendering;

internal sealed class DisplayModeManager(GraphicsDeviceManager graphics, RenderScaler renderScaler)
    : IDisplayModeManager
{
    public void InitializeDefault() => SetWindowed();

    public void ToggleFullscreen()
    {
        if (graphics.IsFullScreen) SetWindowed();
        else SetBorderlessFullscreen();
    }

    private void SetWindowed()
    {
        const int width = 1920;
        const int height = 1080;
        graphics.IsFullScreen = false;
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = height;
        graphics.HardwareModeSwitch = false;

        Apply();
    }

    private void SetBorderlessFullscreen()
    {
        var mode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

        graphics.IsFullScreen = true;
        graphics.HardwareModeSwitch = false;
        graphics.PreferredBackBufferWidth = mode.Width;
        graphics.PreferredBackBufferHeight = mode.Height;

        Apply();
    }

    private void Apply()
    {
        graphics.ApplyChanges();
        renderScaler.UpdateOutputRectangle();
    }
}