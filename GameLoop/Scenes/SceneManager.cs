using System;
using System.Collections.Generic;

namespace GameLoop.Scenes;

internal class SceneManager : IDisposable
{
    private readonly Stack<(IScene Scene, IDisposable? Scope)> _sceneStack = new();
    private IDisposable? _currentScope;

    internal IScene? Current { get; private set; }
    internal int InputFramesToSkip { get; set; }

    public void Dispose()
    {
        DisposeCurrent();
        while (_sceneStack.TryPop(out var item))
        {
            item.Scene.Dispose();
            item.Scope?.Dispose();
        }
    }

    internal void Push(IScene scene, IDisposable? sceneDiScope = null)
    {
        if (Current != null)
            _sceneStack.Push((Current, _currentScope));

        Current = scene;
        _currentScope = sceneDiScope;

        InputFramesToSkip = 1; // Prevents within-frame race conditions with other input managers
    }

    internal void Pop()
    {
        DisposeCurrent();

        if (_sceneStack.Count > 0)
        {
            var prev = _sceneStack.Pop();
            Current = prev.Scene;
            _currentScope = prev.Scope;
        }

        InputFramesToSkip = 1;
    }

    internal void ClearAndSet(IScene scene, IDisposable? sceneDiScope = null)
    {
        DisposeCurrent();

        while (_sceneStack.TryPop(out var item))
        {
            item.Scene.Dispose();
            item.Scope?.Dispose();
        }

        Current = scene;
        _currentScope = sceneDiScope;

        InputFramesToSkip = 1;
    }


    private void DisposeCurrent()
    {
        Current?.Dispose();
        _currentScope?.Dispose();
    }
}