namespace Gameplay;

public interface IAppLifeCycle
{
    bool CanExit { get; }
    /// <summary>
    ///     Should only be called when <see cref="CanExit" /> returns true. Throws otherwise
    /// </summary>
    void Exit();
}