namespace Gameplay.Rendering.Effects.SpriteBatch;

/// <summary>
///     Base type for sprite-batch effects (things that map to spriteBatch.Begin(effect: ...))
/// </summary>
public abstract record SpriteBatchEffect
{
    public static GreyscaleEffect Greyscale { get; } = new();
}