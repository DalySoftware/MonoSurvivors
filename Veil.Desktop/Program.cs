using System;
using GameLoop;
using Veil.Desktop.PlatformServices;

namespace Veil.Desktop;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        using var game = new CoreGame(ServiceConfigurator.Configure);
        game.Run();
    }
}