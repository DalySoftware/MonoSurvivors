using Gameplay.Audio;

namespace Veil.Web.PlatformServices.Audio;

public class NullSoundEffectPlayer : IAudioPlayer
{
    public void Play(SoundEffectTypes effectType) { }
}