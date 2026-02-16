using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace GameLoop.Audio.Music;

internal sealed class MusicDirector
{
    private readonly static TimeSpan SwitchInterval = TimeSpan.FromMinutes(8);

    private readonly IMusicModule[] _modules;
    private int _moduleIndex;
    private MusicTier _tier;

    // > 0 : counting down
    // <= 0: switch armed; will occur on next loop boundary
    private TimeSpan _untilSwitch = SwitchInterval;
    private bool _switchedThisBoundary;
    private readonly IMusicTierPolicy _tierPolicy;
    public MusicDirector(IEnumerable<IMusicModule> modules, IMusicTierPolicy tierPolicy)
    {
        _tierPolicy = tierPolicy;
        _modules = modules.ToArray();
        _tier = MusicTier.Ambient;

        _moduleIndex = 0;
        _modules[_moduleIndex].Activate(_tier);
    }

    private IMusicModule Module => _modules[_moduleIndex];

    public IReadOnlyDictionary<ushort, string> Bindings => Module.Bindings;
    public TimeSpan LoopDuration => Module.LoopDuration;

    internal bool ConsumeSwitchedThisBoundary()
    {
        var v = _switchedThisBoundary;
        _switchedThisBoundary = false;
        return v;
    }

    internal void SetTier(MusicTier tier)
    {
        _tier = tier;
        Module.SetTier(tier);
    }

    internal void Update(GameTime gameTime)
    {
        if (_modules.Length <= 1)
            return;

        // If already armed, no need to keep decrementing.
        if (_untilSwitch <= TimeSpan.Zero)
            return;

        _untilSwitch -= gameTime.ElapsedGameTime;
    }

    public void OnLoopBoundary(long boundaryIndex)
    {
        // Switch only on boundaries (musically clean).
        if (_untilSwitch <= TimeSpan.Zero && _modules.Length > 1)
            SwitchModule();

        if (_tier != MusicTier.Ambient)
        {
            _tierPolicy.OnLoopBoundary();
            var desired = _tierPolicy.DecideTier(_tier);
            if (desired != _tier)
                SetTier(desired);
        }

        Module.OnLoopBoundary(boundaryIndex);
    }
    private void SwitchModule()
    {
        _untilSwitch = SwitchInterval;

        _moduleIndex = (_moduleIndex + 1) % _modules.Length;

        Module.Activate(_tier);

        _switchedThisBoundary = true;
    }

    public void GetVolumes(Span<float> volumes) => Module.GetVolumes(volumes);
}