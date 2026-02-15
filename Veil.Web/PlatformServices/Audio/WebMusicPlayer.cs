using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLoop.Audio.Music;
using GameLoop.Exceptions;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Microsoft.JSInterop;
using Microsoft.Xna.Framework;

namespace Veil.Web.PlatformServices.Audio;

public sealed class WebMusicPlayer : IMusicPlayer, IDisposable
{
    private readonly IJSRuntime _js;
    private readonly ISettingsPersistence _settingsPersistence;
    private readonly MusicDucker _ducking;
    private readonly AsyncPump _asyncPump;

    private readonly Dictionary<ushort, Task> _queueByChannel = new();

    private bool _started;

    private object[]? _pendingStartPayload;
    private readonly Dictionary<ushort, float> _pendingVolumeByChannel = new();

    private bool _flushInFlight;


    public WebMusicPlayer(IJSRuntime js, ISettingsPersistence settingsPersistence, MusicDucker ducking,
        AsyncPump asyncPump)
    {
        _js = js;
        _settingsPersistence = settingsPersistence;
        _ducking = ducking;
        _asyncPump = asyncPump;

        _ducking.OnEffectiveVolumeChanged += OnEffectiveVolumeChanged;

        UpdateVolume(settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings));
        settingsPersistence.OnChanged -= OnSettingsChange;
        settingsPersistence.OnChanged += OnSettingsChange;
    }

    public void Dispose()
    {
        _settingsPersistence.OnChanged -= OnSettingsChange;
        _ducking.OnEffectiveVolumeChanged -= OnEffectiveVolumeChanged;

        _ = SafeStopAllAsync(); // best-effort teardown
    }

    public void Update(GameTime gameTime) =>
        // Keep trying; it will succeed right after the first user gesture.
        _asyncPump.Track(TryFlushAsync());

    private async Task TryFlushAsync()
    {
        if (_flushInFlight)
            return;

        _flushInFlight = true;
        try
        {
            // If already started, keep master volume up to date and we're done.
            if (_started)
            {
                await _js.InvokeVoidAsync("veilAudio.setMusicMasterVolume", _ducking.EffectiveVolume);
                return;
            }

            // Attempt to resume audio; will return false until a user gesture.
            var resumed = await _js.InvokeAsync<bool>("veilAudio.tryResumeAudio");
            if (!resumed)
                return;

            // We have an active AudioContext now. Apply master volume first.
            await _js.InvokeVoidAsync("veilAudio.setMusicMasterVolume", _ducking.EffectiveVolume);

            // Start module if we have one pending.
            if (_pendingStartPayload is not null)
            {
                await _js.InvokeVoidAsync("veilAudio.startModule", [_pendingStartPayload]);
                _pendingStartPayload = null;
            }

            // Push any cached per-channel volumes.
            if (_pendingVolumeByChannel.Count > 0)
            {
                foreach (var (channel, vol) in _pendingVolumeByChannel)
                    await _js.InvokeVoidAsync("veilAudio.setMusicChannelVolume", channel, vol);

                _pendingVolumeByChannel.Clear();
            }

            _started = true;
        }
        finally
        {
            _flushInFlight = false;
        }
    }


    private ValueTask Enqueue(ushort channel, Func<ValueTask> work)
    {
        if (!_queueByChannel.TryGetValue(channel, out var q))
            q = Task.CompletedTask;

        q = q.ContinueWith(_ => work().AsTask(), TaskScheduler.Default).Unwrap();
        _queueByChannel[channel] = q;

        return new ValueTask(q);
    }

    public async ValueTask StartModule(IReadOnlyList<(ushort Channel, string StemKey)> bindings)
    {
        var payload = new object[bindings.Count];
        for (var i = 0; i < bindings.Count; i++)
            payload[i] = new
            {
                channel = (int)bindings[i].Channel,
                url = ResolveWebUrl(bindings[i].StemKey),
            };

        _pendingStartPayload = payload;

        // Try immediately (may no-op if not resumed yet)
        await TryFlushAsync();
    }

    public ValueTask Stop(ushort channel, TimeSpan fadeDuration) =>
        Enqueue(channel, () => StopCore(channel, fadeDuration));

    private async ValueTask StopCore(ushort channel, TimeSpan fadeDuration)
    {
        if (fadeDuration <= TimeSpan.Zero)
        {
            await _js.InvokeVoidAsync("veilAudio.stopMusicChannel", channel);
            return;
        }

        var fadeSeconds = Math.Max(0.01, fadeDuration.TotalSeconds);
        await _js.InvokeVoidAsync("veilAudio.fadeOutAndStopChannel", channel, fadeSeconds);
    }

    public async ValueTask SetChannelVolume(ushort channel, float volumeMultiplier)
    {
        volumeMultiplier = MathHelper.Clamp(volumeMultiplier, 0, 1);

        // Cache always; if we're not started yet, this prevents the "startModule resets to 0" issue.
        _pendingVolumeByChannel[channel] = volumeMultiplier;

        if (_started)
            await _js.InvokeVoidAsync("veilAudio.setMusicChannelVolume", channel, volumeMultiplier);
        else
            await TryFlushAsync();
    }


    public async Task DuckFor(TimeSpan? duration = null, float duckFactor = 0.7f)
    {
        duration ??= TimeSpan.FromMilliseconds(100);

        using (_ducking.BeginTemporaryDuck(duckFactor))
        {
            await Task.Delay(duration.Value);
        }
    }

    private static string ResolveWebUrl(string stemKey) => "music/" + stemKey + ".wav";

    private void OnSettingsChange(Type type)
    {
        if (type != typeof(AudioSettings)) return;
        UpdateVolume(_settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings));
    }

    private void UpdateVolume(AudioSettings settings)
    {
        var baseVolume = settings.MasterVolume * settings.MusicVolume;
        _ducking.SetBaseVolume(baseVolume);
    }

    private void OnEffectiveVolumeChanged(float volume) => _asyncPump.Track(TryFlushAsync());

    private async Task SafeStopAllAsync()
    {
        try
        {
            if (_started)
                await _js.InvokeVoidAsync("veilAudio.stopAllMusic");
        }
        catch
        {
            // ignore during teardown
        }
        finally
        {
            _started = false;
            _queueByChannel.Clear();
        }
    }
}