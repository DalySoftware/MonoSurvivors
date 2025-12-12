namespace Gameplay.Behaviour;

public interface IHasPosition
{
    /// <summary>
    ///     For most (all?) entities, this is the centre point
    /// </summary>
    public Vector2 Position { get; }
}