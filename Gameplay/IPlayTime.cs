using System;

namespace Gameplay;

public interface IPlayTime
{
    TimeSpan TimeSinceRunStart { get; }
}