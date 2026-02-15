using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameLoop.Exceptions;
using Gameplay.Audio;
using Microsoft.Xna.Framework;

namespace GameLoop.Audio.Music;

/// <summary>
///     Drives a fixed-duration musical grid (module.LoopDuration) and applies the module's
///     per-channel volumes.
/// </summary>
internal sealed class MusicTransport(IMusicPlayer player, AsyncPump asyncPump, MusicDirector director)
{
    private readonly static TimeSpan ModuleSwitchFade = TimeSpan.FromSeconds(2);

    private readonly Dictionary<ushort, ChannelState> _channels = new();

    private bool _started;
    private long _boundaryIndex;
    private TimeSpan _sinceBoundary = TimeSpan.Zero;

    private bool _applyInFlight;
    private bool _applyDirty;
    private bool _switchInFlight;


    // Reused buffer for per-binding volumes.
    private float[] _volumes = [];

    internal async ValueTask StartAsync(CancellationToken ct = default)
    {
        _started = true;
        _boundaryIndex = 0;
        _sinceBoundary = TimeSpan.Zero;

        // Director delegates any needed calls to the modules
        director.OnLoopBoundary(_boundaryIndex);

        // Ensure all bindings are started once.
        await StartModuleBindingsAsync(ct);

        // Apply initial volumes immediately.
        await ApplyVolumesAsync(ct);
    }

    internal void Update(GameTime gameTime)
    {
        if (!_started)
            return;

        _sinceBoundary += gameTime.ElapsedGameTime;

        var loop = director.LoopDuration;
        var boundaryAdvanced = false;

        while (_sinceBoundary >= loop)
        {
            _sinceBoundary -= loop;
            _boundaryIndex++;
            director.OnLoopBoundary(_boundaryIndex);
            boundaryAdvanced = true;

            // If the director switched modules on this boundary,
            // perform the transport-level switch.
            if (!_switchInFlight && director.ConsumeSwitchedThisBoundary())
            {
                _switchInFlight = true;
                asyncPump.Track(SwitchModuleAsync());
                return;
            }
        }

        if (boundaryAdvanced)
            RequestApplyVolumes();
    }

    private async ValueTask SwitchModuleAsync()
    {
        // 1) Fade out and stop all current channels.
        foreach (var channel in _channels.Keys)
            await player.Stop(channel, ModuleSwitchFade);

        // Wait for fade to complete before starting new module.
        await Task.Delay(ModuleSwitchFade);

        _channels.Clear();

        // 2) Reset musical phase for new module.
        _boundaryIndex = 0;
        _sinceBoundary = TimeSpan.Zero;

        // 3) Let director initialise the new module at boundary 0.
        director.OnLoopBoundary(_boundaryIndex);

        // 4) Start new bindings.
        await StartModuleBindingsAsync(CancellationToken.None);

        // 5) Apply initial volumes (ramps will fade in).
        await ApplyVolumesAsync(CancellationToken.None);

        _switchInFlight = false;
    }

    internal void RequestApplyVolumes()
    {
        if (_applyInFlight)
        {
            _applyDirty = true;
            return;
        }

        _applyInFlight = true;
        asyncPump.Track(ApplyVolumesPumpAsync());
    }

    private async ValueTask ApplyVolumesPumpAsync()
    {
        try
        {
            while (true)
            {
                _applyDirty = false;

                await ApplyVolumesAsync(CancellationToken.None);

                if (!_applyDirty)
                    return;
            }
        }
        finally
        {
            _applyInFlight = false;
        }
    }

    private async ValueTask StartModuleBindingsAsync(CancellationToken ct)
    {
        var bindings = director.Bindings;

        // Build the simple tuple list once.
        var start = new (ushort Channel, string StemKey)[bindings.Count];

        foreach (var (channel, stemKey) in bindings)
        {
            ct.ThrowIfCancellationRequested();

            // Track locally (same invariant checks you already had)
            if (_channels.TryGetValue(channel, out var existing) &&
                !string.Equals(existing.StemKey, stemKey, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"Channel bindings must be stable. Channel {channel} changed stem from '{existing.StemKey}' to '{stemKey}'.");

            _channels[channel] = new ChannelState(stemKey) { LastVolume = float.NaN };
            start[channel] = (channel, stemKey);
        }

        await player.StartModule(start);
    }


    private async ValueTask ApplyVolumesAsync(CancellationToken ct)
    {
        var bindings = director.Bindings;
        var count = bindings.Count;

        if (_volumes.Length < count)
            _volumes = new float[Math.Max(count, _volumes.Length * 2)];

        director.GetVolumes(_volumes.AsSpan(0, count));

        foreach (var (channel, _) in bindings)
        {
            ct.ThrowIfCancellationRequested();

            var vol = MathHelper.Clamp(_volumes[channel], 0f, 1f);

            // Should exist if EnsureBindingsStartedAsync ran, but keep it defensive.
            if (!_channels.TryGetValue(channel, out var state))
                continue;

            if (NearlyEqual(state.LastVolume, vol))
                continue;

            await player.SetChannelVolume(channel, vol);
            state.LastVolume = vol;
        }
    }

    private static bool NearlyEqual(float a, float b) => MathF.Abs(a - b) <= 0.0005f;

    private sealed class ChannelState(string stemKey)
    {
        internal string StemKey { get; } = stemKey;

        // Cached last-applied channel volume (0..1). NaN means "unknown / not yet applied".
        internal float LastVolume { get; set; } = float.NaN;
    }
}