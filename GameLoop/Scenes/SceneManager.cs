using System;
using System.Collections.Generic;

namespace GameLoop.Scenes;

internal class SceneManager : IDisposable
{
    private readonly Stack<IScene> _sceneStack = new();

    internal IScene? Current { get; private set; }
    internal int InputFramesToSkip { get; set; }

    public void Dispose()
    {
        Current?.Dispose();
        while (_sceneStack.TryPop(out var scene))
            scene.Dispose();
    }

    internal void Push(IScene scene)
    {
        if (Current != null)
            _sceneStack.Push(Current);
        Current = scene;

        InputFramesToSkip = 1; // Prevents within-frame race conditions with other input managers
    }

    internal void Pop()
    {
        Current?.Dispose();
        if (_sceneStack.Count > 0)
            Current = _sceneStack.Pop();

        InputFramesToSkip = 1; // Prevents within-frame race conditions with other input managers
    }
}