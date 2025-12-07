using Gameplay.Behaviour;

namespace Gameplay.Rendering;

/// <summary>
///     Declares the visual representation (texture path) for an entity
/// </summary>
public interface IVisual : IHasPosition
{
    /// <summary>
    ///     The content path to the texture asset for this entity
    /// </summary>
    string TexturePath { get; }
}
