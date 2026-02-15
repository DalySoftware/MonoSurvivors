namespace GameLoop.Audio.Music;

internal interface IMusicTierPolicy
{
    MusicTier DecideTier(MusicTier currentTier);
    void OnLoopBoundary();
}

internal sealed class NullTierPolicy : IMusicTierPolicy
{
    public MusicTier DecideTier(MusicTier currentTier) => currentTier;
    public void OnLoopBoundary() { }
}