using GameLoop.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;

namespace Veil.Desktop.PlatformServices.Rendering;

public class DesktopBackground : IBackground
{
    public Color Color { get; } = ColorPalette.ItchBranding;
}