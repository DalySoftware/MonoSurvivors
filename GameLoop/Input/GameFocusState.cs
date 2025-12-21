using System;

namespace GameLoop.Input;

internal sealed class GameFocusState
{
    private bool _hasFocus;

    public bool HasFocus
    {
        get => _hasFocus;
        set
        {
            var gainedFocus = value && !_hasFocus;
            if (gainedFocus) GainedFocus?.Invoke();

            _hasFocus = value;
        }
    }

    public event Action? GainedFocus;
}