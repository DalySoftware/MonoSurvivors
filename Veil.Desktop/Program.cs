using System;

namespace Veil.Desktop;

public static class Program
{
    [STAThread]
    static void Main()
    {
        using var game = new GameLoop.CoreGame();
        game.Run();
    }
}
