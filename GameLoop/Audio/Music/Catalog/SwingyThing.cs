using System;
using System.Collections.Generic;

namespace GameLoop.Audio.Music.Catalog;

public class SwingyThing : IMusicModule
{
    public TimeSpan LoopDuration { get; } = TimeSpan.FromSeconds(107.520);

    public IReadOnlyDictionary<ushort, string> Bindings { get; } = new Dictionary<ushort, string>
    {
        { 0, "SwingyThing/WholeSong" },
    };

    public void SetTier(MusicTier tier) { }
    public void OnLoopBoundary(long boundaryIndex) { }
    public void GetVolumes(Span<float> volumes)
    {
        volumes.Clear();
        volumes[0] = 1f;
    }
}