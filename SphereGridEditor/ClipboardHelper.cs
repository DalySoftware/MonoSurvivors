using System.Diagnostics;

namespace SphereGridEditor;

public static class ClipboardHelper
{
    public static void Copy(string text)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "clip",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };
        process.Start();
        process.StandardInput.Write(text);
        process.StandardInput.Close();
        process.WaitForExit();
    }
}