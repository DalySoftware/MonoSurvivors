using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLoop.Audio.Music;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Veil.Desktop.PlatformServices.Audio;

internal sealed class MusicPlayer : IDisposable, IMusicPlayer
{
    private readonly ContentManager _content;
    private readonly MusicDucker _ducking;
    private readonly ISettingsPersistence _settingsPersistence;

    private readonly Dictionary<string, SoundEffect> _effects = new();
    private readonly Dictionary<ushort, ChannelState> _channels = new();

    private float _masterVolume;

    public MusicPlayer(ContentManager content, MusicDucker ducking, ISettingsPersistence settingsPersistence)
    {
        _content = content;
        _ducking = ducking;
        _settingsPersistence = settingsPersistence;

        _ducking.OnEffectiveVolumeChanged += OnEffectiveVolumeChanged;

        UpdateBaseVolume(settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings));

        settingsPersistence.OnChanged -= OnSettingsChange;
        settingsPersistence.OnChanged += OnSettingsChange;

        _masterVolume = MathHelper.Clamp(_ducking.EffectiveVolume, 0f, 1f);
    }

    public void Dispose()
    {
        _settingsPersistence.OnChanged -= OnSettingsChange;
        _ducking.OnEffectiveVolumeChanged -= OnEffectiveVolumeChanged;

        foreach (var ch in _channels.Values)
            StopAndDisposeChannel(ch);

        _channels.Clear();
        _effects.Clear();
    }

    public void Update(GameTime gameTime)
    {
        var dt = gameTime.ElapsedGameTime;
        foreach (var ch in _channels.Values)
        {
            UpdateChannelFade(ch, dt);
            UpdateChannelRamp(ch, dt);
        }
    }

    public ValueTask StartModule(IReadOnlyList<(ushort Channel, string StemKey)> bindings)
    {
        // 1) Create/replace instances first (no Play() yet)
        for (var i = 0; i < bindings.Count; i++)
        {
            var (channel, stemKey) = bindings[i];
            var ch = GetChannel(channel);

            // Immediate replace (no fade) as part of module start.
            ch.Mode = FadeMode.None;
            ch.FadeElapsed = TimeSpan.Zero;
            ch.FadeDuration = TimeSpan.Zero;

            StopAndDispose(ref ch.Instance);

            ch.Instance = CreateInstance(stemKey);

            // Module can set channel volumes immediately after.
            ch.Gain = 1f;

            ApplyChannelVolume(ch);
        }

        // 2) Then start them all (best-effort "together")
        for (var i = 0; i < bindings.Count; i++)
        {
            var (channel, _) = bindings[i];
            var ch = GetChannel(channel);

            // Defensive: shouldn't be null, but don't explode here.
            ch.Instance?.Play();
        }

        return ValueTask.CompletedTask;
    }


    public ValueTask Stop(ushort channel, TimeSpan fadeDuration)
    {
        var ch = GetChannel(channel);
        if (ch.Instance == null)
            return ValueTask.CompletedTask;

        if (fadeDuration <= TimeSpan.Zero)
        {
            StopAndDispose(ref ch.Instance);

            ch.Gain = 0f;
            ch.Mode = FadeMode.None;
            ch.FadeElapsed = TimeSpan.Zero;
            ch.FadeDuration = TimeSpan.Zero;

            return ValueTask.CompletedTask;
        }

        ch.Mode = FadeMode.FadeOut;
        ch.FadeElapsed = TimeSpan.Zero;
        ch.FadeDuration = NormalizeFade(fadeDuration);

        // Capture current gain so repeated Stop() calls behave sensibly.
        ch.Gain = MathHelper.Clamp(ch.Gain, 0f, 1f);
        ApplyChannelVolume(ch);

        return ValueTask.CompletedTask;
    }

    public ValueTask SetChannelVolume(ushort channel, float volumeMultiplier)
    {
        var ch = GetChannel(channel);
        ch.TargetChannelVolume = MathHelper.Clamp(volumeMultiplier, 0f, 1f);

        // Apply immediately in case ramp is effectively disabled (or very short),
        // and to make new channels react without waiting a frame.
        ApplyChannelVolume(ch);
        return ValueTask.CompletedTask;
    }

    public async Task DuckFor(TimeSpan? duration = null, float duckFactor = 0.7f)
    {
        duration ??= TimeSpan.FromMilliseconds(100);

        using (_ducking.BeginTemporaryDuck(duckFactor))
        {
            await Task.Delay(duration.Value).ConfigureAwait(false);
        }
    }

    // ---------------- internals ----------------

    private static TimeSpan NormalizeFade(TimeSpan fade)
        => fade <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : fade;

    private ChannelState GetChannel(ushort channel)
    {
        if (_channels.TryGetValue(channel, out var existing))
            return existing;

        var created = new ChannelState();
        _channels.Add(channel, created);
        ApplyChannelVolume(created);
        return created;
    }

    private void UpdateChannelFade(ChannelState ch, TimeSpan dt)
    {
        if (ch.Mode == FadeMode.None)
            return;

        ch.FadeElapsed += dt;

        var t = (float)(ch.FadeElapsed.TotalSeconds / ch.FadeDuration.TotalSeconds);
        if (t >= 1f)
        {
            FinishFade(ch);
            return;
        }

        t = MathHelper.Clamp(t, 0f, 1f);

        // FadeOut
        ch.Gain = 1f - t;
        ApplyChannelVolume(ch);
    }

    private void UpdateChannelRamp(ChannelState ch, TimeSpan dt)
    {
        // If instance doesn't exist, nothing to apply; but keep state so it resumes correctly.
        if (ch.Instance == null)
            return;

        var target = MathHelper.Clamp(ch.TargetChannelVolume, 0f, 1f);
        var current = MathHelper.Clamp(ch.ChannelVolume, 0f, 1f);

        // Exponential smoothing toward target.
        // alpha = 1 - exp(-dt/tau)
        var dtSeconds = (float)dt.TotalSeconds;
        if (dtSeconds <= 0f)
            return;

        var tau = target < current ? IMusicPlayer.StemRampDownConstantSeconds : IMusicPlayer.StemRampUpConstantSeconds;
        var alpha = 1f - MathF.Exp(-dtSeconds / tau);
        var next = current + (target - current) * alpha;

        // Snap to 0 when very quiet
        if (target == 0f && next <= 0.02f)
            next = 0f;

        ch.ChannelVolume = next;
        ApplyChannelVolume(ch);
    }

    private static void FinishFade(ChannelState ch)
    {
        // FadeOut only
        StopAndDispose(ref ch.Instance);

        ch.Gain = 0f;

        ch.Mode = FadeMode.None;
        ch.FadeElapsed = TimeSpan.Zero;
        ch.FadeDuration = TimeSpan.Zero;
    }

    private void ApplyChannelVolume(ChannelState ch)
    {
        var instance = ch.Instance;
        if (instance == null) return;

        var channelGain = MathHelper.Clamp(_masterVolume * ch.ChannelVolume * ch.Gain, 0f, 1f);
        instance.Volume = channelGain;
    }

    private void StopAndDisposeChannel(ChannelState ch)
    {
        StopAndDispose(ref ch.Instance);

        ch.ChannelVolume = 0f;
        ch.TargetChannelVolume = 0f;
        ch.Gain = 0f;

        ch.Mode = FadeMode.None;
        ch.FadeElapsed = TimeSpan.Zero;
        ch.FadeDuration = TimeSpan.Zero;
    }

    private SoundEffectInstance CreateInstance(string stemKey)
    {
        var effect = GetOrLoadEffect(stemKey);
        var instance = effect.CreateInstance();
        instance.IsLooped = true;
        return instance;
    }

    private SoundEffect GetOrLoadEffect(string stemKey)
    {
        if (_effects.TryGetValue(stemKey, out var existing))
            return existing;

        var effect = _content.Load<SoundEffect>(ResolveDesktopContentName(stemKey));
        _effects.Add(stemKey, effect);
        return effect;
    }

    private static string ResolveDesktopContentName(string stemKey)
        => @"Music\" + stemKey.Replace('/', '\\');

    private void OnSettingsChange(Type type)
    {
        if (type != typeof(AudioSettings)) return;
        UpdateBaseVolume(_settingsPersistence.Load(PersistenceJsonContext.Default.AudioSettings));
    }

    private void UpdateBaseVolume(AudioSettings settings)
    {
        var baseVolume = settings.MasterVolume * settings.MusicVolume;
        _ducking.SetBaseVolume(baseVolume);
    }

    private void OnEffectiveVolumeChanged(float volume)
    {
        _masterVolume = MathHelper.Clamp(volume, 0f, 1f);
        foreach (var ch in _channels.Values)
            ApplyChannelVolume(ch);
    }

    private static void StopAndDispose(ref SoundEffectInstance? instance)
    {
        if (instance == null) return;

        try
        {
            instance.Stop();
        }
        catch
        {
            /* ignored */
        }

        try
        {
            instance.Dispose();
        }
        catch
        {
            /* ignored */
        }

        instance = null;
    }

    private sealed class ChannelState
    {
        public SoundEffectInstance? Instance;

        // Current (ramped) channel volume multiplier.
        public float ChannelVolume { get; set; } = 0f;

        // Target requested by transport.
        public float TargetChannelVolume { get; set; } = 0f;

        // Current fade gain (1 -> 0 during FadeOut).
        public float Gain { get; set; } = 0f;

        public FadeMode Mode { get; set; } = FadeMode.None;

        public TimeSpan FadeElapsed { get; set; } = TimeSpan.Zero;
        public TimeSpan FadeDuration { get; set; } = TimeSpan.Zero;
    }

    private enum FadeMode
    {
        None,
        FadeOut,
    }
}