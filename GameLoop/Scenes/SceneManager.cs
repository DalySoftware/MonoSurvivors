namespace GameLoop.Scenes;

internal class SceneManager(IScene? initial)
{
    internal IScene? Current { get; private set; } = initial;

    internal void Switch(IScene scene)
    {
        var old = Current;
        Current = scene;
        old?.Dispose();
    }
}