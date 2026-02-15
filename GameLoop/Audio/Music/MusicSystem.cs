using GameLoop.Exceptions;
using Gameplay.Audio;
using Microsoft.Xna.Framework;

namespace GameLoop.Audio.Music;

/// <summary>
///     Central entry point for music subsystems
/// </summary>
internal sealed class MusicSystem(
    MusicDirector director,
    MusicTransport transport,
    IMusicPlayer player,
    AsyncPump asyncPump)
{
    private bool _started;

    internal void Start()
    {
        if (_started) return;

        _started = true;
        asyncPump.Track(transport.StartAsync());
    }

    internal void SetTier(MusicTier tier) => director.SetTier(tier);

    internal void Update(GameTime gameTime)
    {
        if (!_started) return;

        transport.Update(gameTime);
        player.Update(gameTime);
    }
}