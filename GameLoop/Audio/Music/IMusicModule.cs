using System;
using System.Collections.Generic;

namespace GameLoop.Audio.Music;

/// <summary>
///     A module owns the musical structure for a song/set: stem selection and mixing.
///     It outputs a complete mix snapshot the engine can apply.
/// </summary>
internal interface IMusicModule
{
    /// <summary>
    ///     Fixed musical loop duration used by the transport grid.
    ///     Example: Venezuela 8 bars @125 BPM = 15.360s.
    /// </summary>
    TimeSpan LoopDuration { get; }

    /// <summary>Stable for the lifetime of the module instance.</summary>
    IReadOnlyDictionary<ushort, string> Bindings { get; }

    /// <summary>
    ///     Called when the module becomes the active module (after a switch or initial start).
    ///     Use to reset transient state so the first boundary produces a valid snapshot.
    /// </summary>
    /// <param name="tier">Tier to use on activate</param>
    void Activate(MusicTier tier);

    /// <summary>
    ///     Called when game state changes (routing only).
    ///     Module stores whatever it needs for next snapshot.
    /// </summary>
    void SetTier(MusicTier tier);

    /// <summary>
    ///     Called exactly on loop boundary ticks (0, 1, 2, ...).
    ///     Module can advance its internal pattern here.
    /// </summary>
    void OnLoopBoundary(long boundaryIndex);

    /// <summary>
    ///     Writes per-channel volumes (0..1), indexed by channel id.
    ///     Channels are dense 0..Bindings.Count-1 by convention.
    /// </summary>
    void GetVolumes(Span<float> volumes);
}

public enum MusicTier
{
    Ambient,
    Soft,
    Core,
    Peak,
}