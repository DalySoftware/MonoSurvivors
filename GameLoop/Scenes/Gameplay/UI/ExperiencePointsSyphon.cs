using System;
using Microsoft.Xna.Framework;

namespace GameLoop.Scenes.Gameplay.UI;

internal sealed class ExperiencePointsSyphon(TimeSpan siphonDuration, TimeSpan receivePulseDuration, int initialPoints)
{
    private readonly TimeSpan _siphonDuration =
        siphonDuration <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : siphonDuration;
    private readonly TimeSpan _receivePulseDuration =
        receivePulseDuration <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : receivePulseDuration;

    private int _pendingPoints = initialPoints;

    private TimeSpan _elapsedSiphonTime = TimeSpan.Zero;

    // 0..1, decays to 0.

    internal int DisplayPoints { get; private set; } = initialPoints;

    internal bool IsDraining { get; private set; }

    /// <summary>
    ///     0..1. Use this as the bar Start while siphoning: Start = DrainStartFraction; End = 1.
    /// </summary>
    internal float DrainStartFraction =>
        IsDraining ? EaseInOutCubic(ElapsedFraction(_elapsedSiphonTime, _siphonDuration)) : 0f;

    /// <summary>
    ///     0..1. A gentle "charging" signal while siphoning (good for tint/scale).
    /// </summary>
    internal float ChargeStrength => IsDraining ? DrainStartFraction : 0f;

    /// <summary>
    ///     0..1. Kicks up when points are received, then decays.
    /// </summary>
    internal float ReceivePulseStrength { get; private set; }

    internal void Update(int actualPoints, TimeSpan elapsedTime)
    {
        if (elapsedTime < TimeSpan.Zero)
            elapsedTime = TimeSpan.Zero;

        // If points were spent (decrease), snap immediately (no siphon).
        if (actualPoints < DisplayPoints)
        {
            SnapTo(actualPoints);
            return;
        }

        // If points increased, mark pending. Start siphon if not already running.
        if (actualPoints > _pendingPoints)
        {
            _pendingPoints = actualPoints;

            if (!IsDraining)
                StartSiphon();
            else
                // Subtle "still receiving" nudge, kept small.
                ReceivePulseStrength = MathHelper.Clamp(ReceivePulseStrength + 0.10f, 0f, 1f);
        }

        if (IsDraining)
        {
            _elapsedSiphonTime += elapsedTime;

            if (_elapsedSiphonTime >= _siphonDuration)
                FinishSiphon();
        }

        DecayReceivePulse(elapsedTime);
    }

    private void StartSiphon()
    {
        IsDraining = true;
        _elapsedSiphonTime = TimeSpan.Zero;

        // Tiny pre-charge so the box responds immediately.
        ReceivePulseStrength = MathHelper.Clamp(ReceivePulseStrength + 0.15f, 0f, 1f);
    }

    private void FinishSiphon()
    {
        IsDraining = false;
        _elapsedSiphonTime = TimeSpan.Zero;

        if (DisplayPoints == _pendingPoints)
            return;

        DisplayPoints = _pendingPoints;
        ReceivePulseStrength = 1f;
    }

    private void SnapTo(int points)
    {
        DisplayPoints = points;
        _pendingPoints = points;

        IsDraining = false;
        _elapsedSiphonTime = TimeSpan.Zero;
        ReceivePulseStrength = 0f;
    }

    private void DecayReceivePulse(TimeSpan elapsedTime)
    {
        if (ReceivePulseStrength <= 0f)
            return;

        var decay = (float)(elapsedTime.TotalSeconds / _receivePulseDuration.TotalSeconds);
        ReceivePulseStrength = MathF.Max(0f, ReceivePulseStrength - decay);
    }

    private static float ElapsedFraction(TimeSpan elapsed, TimeSpan duration) =>
        duration <= TimeSpan.Zero
            ? 1f
            : MathHelper.Clamp((float)(elapsed.TotalSeconds / duration.TotalSeconds), 0f, 1f);

    private static float EaseInOutCubic(float t)
    {
        t = MathHelper.Clamp(t, 0f, 1f);
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - MathF.Pow(-2f * t + 2f, 3f) * 0.5f;
    }
}