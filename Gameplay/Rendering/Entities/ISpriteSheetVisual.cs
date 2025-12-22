namespace Gameplay.Rendering;

public interface ISpriteSheetVisual : IVisual
{
    ISpriteSheet SpriteSheet { get; }
    IFrame CurrentFrame { get; }

    /// <summary>
    ///     Provide if the sprite should have an outline. Leave null for no outline
    /// </summary>
    Color? OutlineColor { get; }
}