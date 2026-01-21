using System;
using System.Windows.Forms;

namespace SphereGridEditor;
/// <summary>
/// The main class.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Uncomment this line to enable VR with the nkast.Kni.Platform.WinForms.DX11.OculusOVR package.
        //Microsoft.Xna.Platform.XR.XRFactory.RegisterXRFactory(new Microsoft.Xna.Platform.XR.LibOVR.ConcreteXRFactory());

        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            using var game = new Editor();
            game.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            throw;
        }
    }
}
