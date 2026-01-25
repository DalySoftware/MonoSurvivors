using GameLoop.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;

namespace Veil.Web.PlatformServices.Rendering;

public class WebBackground : IBackground
{
    public Color Color { get; } = ColorPalette.Black;
}