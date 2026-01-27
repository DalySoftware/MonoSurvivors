namespace Gameplay.Rendering.Effects;

/// <summary>
///     Base type for sprite-batch effects (things that map to spriteBatch.Begin(effect: ...))
/// </summary>
public abstract record VisualEffect
{
    public static GreyscaleEffect Greyscale { get; } = new();
}