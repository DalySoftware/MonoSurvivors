using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace GameLoop.Exceptions;

public sealed class AsyncPump
{
    private readonly ConcurrentQueue<ExceptionDispatchInfo> _errors = new();

    /// Register fire-and-forget work so faults are never lost.
    public void Track(Task task)
    {
        if (task.IsCompleted)
        {
            CaptureIfFaulted(task);
            return;
        }

        _ = ObserveAsync(task);
        return;

        async Task ObserveAsync(Task t)
        {
            try
            {
                await t.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Capture(ex);
            }
        }
    }

    public void Track(ValueTask task) => Track(task.AsTask());

    internal void ThrowIfAny()
    {
        if (_errors.TryDequeue(out var edi))
            edi.Throw();
    }

    private void CaptureIfFaulted(Task task)
    {
        if (task is { IsFaulted: true, Exception: not null }) Capture(task.Exception);
    }

    private void Capture(Exception ex)
    {
        if (ex is OperationCanceledException) return;
        _errors.Enqueue(ExceptionDispatchInfo.Capture(ex));
    }
}