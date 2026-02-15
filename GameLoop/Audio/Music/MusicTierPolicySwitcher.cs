using System;

namespace GameLoop.Audio.Music;

internal sealed class MusicTierPolicySwitcher : IMusicTierPolicy
{
    private IMusicTierPolicy _current = new NullTierPolicy();
    private int _version;

    public PolicyRestoreToken Use(IMusicTierPolicy policy)
    {
        var prior = _current;
        var myVersion = ++_version;
        _current = policy;

        return new PolicyRestoreToken(() =>
        {
            if (_version == myVersion)
                _current = prior;
        });
    }

    public MusicTier DecideTier(MusicTier currentTier) => _current.DecideTier(currentTier);
    public void OnLoopBoundary() => _current.OnLoopBoundary();

    internal sealed class PolicyRestoreToken(Action restore) : IDisposable
    {
        private Action? _restore = restore;

        public void Dispose()
        {
            _restore?.Invoke();
            _restore = null;
        }
    }
}