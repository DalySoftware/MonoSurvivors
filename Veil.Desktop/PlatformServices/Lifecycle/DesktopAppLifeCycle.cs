using Gameplay;
using Microsoft.Xna.Framework;

namespace Veil.Desktop.PlatformServices.Lifecycle;

internal sealed class DesktopAppLifeCycle(Game game) : IAppLifeCycle
{
    public bool CanExit => true;
    public void Exit() => game.Exit();
}