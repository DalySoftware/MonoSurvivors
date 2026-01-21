using System;
using System.Threading.Tasks;

namespace Gameplay.Audio;

public interface IMusicPlayer
{
    void PlayBackgroundMusic();
    /// <summary>Reduce music volume temporarily for a ducking effect.</summary>
    Task DuckFor(TimeSpan? duration = null, float duckFactor = 0.7f);
    void DuckBackgroundMusic();
    void RestoreBackgroundMusic();
}