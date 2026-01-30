using System;

namespace Gameplay.Rendering.Effects;

public sealed class CrtGlitchPulse
{
    public float Amount { get; private set; }

    public void Trigger(float add) => Amount = MathF.Min(1f, Amount + add);

    public void Update(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Amount = MathF.Max(0f, Amount - dt * 5f); // fast decay
    }
}