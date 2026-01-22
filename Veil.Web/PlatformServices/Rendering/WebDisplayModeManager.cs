using GameLoop.Rendering;
using Microsoft.JSInterop;

namespace Veil.Web.PlatformServices.Rendering;

public sealed class WebDisplayModeManager(IJSInProcessRuntime js, IViewportSync viewportSync) : IDisplayModeManager
{
    public void InitializeDefault() => viewportSync.ForceRefresh();

    public void ToggleFullscreen()
    {
        js.InvokeVoid("veilGraphics.toggleFullscreen");
        viewportSync.ForceRefresh();
    }
}