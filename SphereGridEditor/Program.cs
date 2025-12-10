using System;
using SphereGridEditor;

try
{
    using var game = new Editor();
    Console.WriteLine("Starting editor...");
    game.Run();
    Console.WriteLine("Editor exited.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    throw;
}