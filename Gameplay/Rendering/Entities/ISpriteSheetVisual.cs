namespace Gameplay.Rendering;

public interface ISpriteSheetVisual : IVisual
{
    ISpriteSheet SpriteSheet { get; }
    IFrame CurrentFrame { get; }
}