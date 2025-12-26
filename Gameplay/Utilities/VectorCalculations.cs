namespace Gameplay.Utilities;

internal static class VectorCalculations
{
    /// <summary>
    ///     Computes a normalized direction toward the target, then scales by speed.
    /// </summary>
    internal static Vector2 Velocity(Vector2 initialPosition, Vector2 target, float speed) =>
        (Vector2)new UnitVector2(target - initialPosition) * speed;
}