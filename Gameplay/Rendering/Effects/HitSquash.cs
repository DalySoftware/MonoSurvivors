using System;

namespace Gameplay.Rendering.Effects;

/// <summary>
///     Simple squash/stretch impulse for "got hit" feedback.
/// </summary>
public sealed class HitSquash
{
    private const float DurationMs = 120f; // 80..140 feels good
    private const float Decay = 10f; // higher = settles faster
    private const float Frequency = 18f; // higher = snappier ring
    private const float SquashFactor = 1.5f; // area-ish compensation

    private float _remainingMs;
    private float _strength;
    private bool _horizontalAxis;

    public Vector2 Scale { get; private set; } = Vector2.One;

    public void Trigger(Vector2 ownerPosition, Vector2 hitSourcePosition, float damage, float maxHealth)
    {
        var dir = ownerPosition - hitSourcePosition;

        // Choose stretch axis based on hit direction (top-down feels nicer than always-horizontal).
        // If dir is (near) zero, keep previous axis.
        if (dir.LengthSquared() > 0.0001f)
            _horizontalAxis = MathF.Abs(dir.X) >= MathF.Abs(dir.Y);

        // Strength: base + scaled by damage fraction (+ extra on lethal).
        var normalized = maxHealth > 0f ? damage / maxHealth : 0f; // approx 0..1
        var strength = 0.06f + normalized * 0.22f;
        _strength = MathHelper.Clamp(strength, 0.06f, 0.20f);

        _remainingMs = DurationMs;

        // Apply immediately so the hit feels instant.
        Apply(0f);
    }

    public void Update(GameTime gameTime)
    {
        if (_remainingMs <= 0f)
        {
            Scale = Vector2.One;
            return;
        }

        _remainingMs = MathF.Max(0f, _remainingMs - (float)gameTime.ElapsedGameTime.TotalMilliseconds);

        var progress01 = 1f - _remainingMs / DurationMs; // 0..1
        Apply(progress01);

        if (_remainingMs <= 0f)
            Scale = Vector2.One;
    }

    private void Apply(float progress01)
    {
        // Damped cosine impulse: starts at max deformation and quickly rings down.
        var osc = MathF.Exp(-progress01 * Decay) * MathF.Cos(progress01 * Frequency);

        var stretch = 1f + _strength * osc;
        var squash = 1f - _strength * SquashFactor * osc;

        Scale = _horizontalAxis
            ? new Vector2(stretch, squash)
            : new Vector2(squash, stretch);
    }
}