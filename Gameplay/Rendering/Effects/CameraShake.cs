namespace Gameplay.Rendering.Effects;

public interface ICameraImpulseSink
{
    void AddImpulse(Vector2 impulse);
}

public sealed class CameraShake : ICameraImpulseSink
{
    public const float MaxShakePx = 40f;

    private Vector2 _offset;
    private Vector2 _velocity;

    public Vector2 Offset => _offset;

    public void AddImpulse(Vector2 impulse)
    {
        if (impulse.LengthSquared() < 0.0001f)
            return;

        _velocity += impulse;
    }

    public void Update(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        const float stiffness = 120f; // return-to-zero
        const float damping = 20f; // kill oscillation

        _velocity += (-_offset * stiffness - _velocity * damping) * dt;
        _offset += _velocity * dt;

        // Clamp
        if (_offset.LengthSquared() > MaxShakePx * MaxShakePx)
            _offset = Vector2.Normalize(_offset) * MaxShakePx;

        // Snap-to-zero
        if (_offset.LengthSquared() < 0.01f * 0.01f &&
            _velocity.LengthSquared() < 0.01f * 0.01f)
        {
            _offset = Vector2.Zero;
            _velocity = Vector2.Zero;
        }
    }
}