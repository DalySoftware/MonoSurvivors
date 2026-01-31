using System;
using Gameplay.Rendering.Colors;

namespace Gameplay.Rendering.Effects;

public sealed class HitFlash
{
    private const float DurationMs = 200f;

    private float _remainingMs;
    private float _strength;

    public float Intensity
    {
        get
        {
            if (_remainingMs <= 0f) return 0f;

            // Ease-out so it pops then fades quickly.
            var t = _remainingMs / DurationMs; // 1..0
            return _strength * t * t;
        }
    }

    public Color Color { get; } = ColorPalette.White.ShiftLightness(-0.2f);

    public void Trigger(float damage, float maxHealth, bool isLethal)
    {
        // Scale a bit by damage fraction; keep a floor so light hits still show.
        var frac = maxHealth > 0f ? MathHelper.Clamp(damage / maxHealth, 0f, 1f) : 0f;
        var bump = MathHelper.Lerp(0.35f, 1.0f, frac);
        if (isLethal) bump = MathF.Min(1f, bump + 0.25f);

        _strength = MathF.Min(1f, _strength + bump * 0.75f);
        _remainingMs = DurationMs;
    }

    public void Update(GameTime gameTime)
    {
        if (_remainingMs <= 0f) return;

        _remainingMs -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        if (_remainingMs <= 0f)
        {
            _remainingMs = 0f;
            _strength = 0f;
        }
    }
}

public interface IHasHitFlash
{
    float FlashIntensity { get; } // 0..1
    Color FlashColor { get; }
}