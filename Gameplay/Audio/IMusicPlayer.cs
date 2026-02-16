using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gameplay.Audio;

public interface IMusicPlayer
{
    // ~63% toward target after these durations.
    public const float StemRampUpConstantSeconds = 4;
    public const float StemRampDownConstantSeconds = 0.6f;

    void Update(GameTime gameTime);

    ValueTask StartModule(IReadOnlyList<(ushort Channel, string StemKey)> bindings);

    /// <summary>Fade out and stop the channel.</summary>
    ValueTask Stop(ushort channel, TimeSpan fadeDuration);

    /// <summary>Per-channel volume multiplier (post-mix, pre-master).</summary>
    ValueTask SetChannelVolume(ushort channel, float volumeMultiplier);

    /// <summary>Reduce music volume temporarily for a ducking effect.</summary>
    Task DuckFor(TimeSpan? duration = null, float duckFactor = 0.7f);
}