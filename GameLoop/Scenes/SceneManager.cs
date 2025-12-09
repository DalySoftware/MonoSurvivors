using System;
using System.Collections.Generic;

namespace GameLoop.Scenes;

internal class SceneManager(IScene? initial) : IDisposable
{
    private readonly Stack<IScene> _sceneStack = new();

    internal IScene? Current { get; private set; } = initial;

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
    }

    internal void Pop()
    {
        Current?.Dispose();
        if (_sceneStack.Count > 0)
            Current = _sceneStack.Pop();
    }
}