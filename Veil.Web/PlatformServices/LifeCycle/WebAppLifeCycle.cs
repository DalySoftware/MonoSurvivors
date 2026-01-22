using System;
using Gameplay;

namespace Veil.Web.PlatformServices.LifeCycle;

internal sealed class WebAppLifecycle : IAppLifeCycle
{
    public bool CanExit => false;

    public void Exit() =>
        throw new WebAppLifeCycleException();

    private class WebAppLifeCycleException() : InvalidOperationException("Exit is not supported on this platform.");
}