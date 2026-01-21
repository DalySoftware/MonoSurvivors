using System;
using System.Threading.Tasks;
using Gameplay.Audio;

namespace Veil.Web.PlatformServices.Audio;

public class NullMusicPlayer : IMusicPlayer
{
    public void PlayBackgroundMusic() { }
    public Task DuckFor(TimeSpan? duration = null, float duckFactor = 0.7f) => Task.Delay(duration ?? TimeSpan.Zero);
    public void DuckBackgroundMusic() { }
    public void RestoreBackgroundMusic() { }
}