using System;
using System.Collections.Generic;

namespace GameLoop.Audio.Music;

internal sealed class MusicDirector(IMusicModule module, IMusicTierPolicy tierPolicy)
{
    private MusicTier _tier = MusicTier.Ambient;

    public IReadOnlyDictionary<ushort, string> Bindings => module.Bindings;
    public TimeSpan LoopDuration => module.LoopDuration;

    internal void SetTier(MusicTier tier)
    {
        _tier = tier;
        module.SetTier(tier);
    }

    public void OnLoopBoundary(long boundaryIndex)
    {
        if (_tier != MusicTier.Ambient)
        {
            tierPolicy.OnLoopBoundary();
            var desired = tierPolicy.DecideTier(_tier);
            if (desired != _tier)
                SetTier(desired);
        }

        module.OnLoopBoundary(boundaryIndex);
    }

    public void GetVolumes(Span<float> volumes) => module.GetVolumes(volumes);
}