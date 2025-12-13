using ContentLibrary;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace GameLoop.Music;

public class MusicPlayer
{
    private readonly SoundEffectInstance _soundEffect;

    public MusicPlayer(ContentManager content)
    {
        _soundEffect = content.Load<SoundEffect>(Paths.Music.AppleStrudel).CreateInstance();
        _soundEffect.IsLooped = true;
    }

    public void PlayBackgroundMusic() => _soundEffect.Play();

    public void DuckBackgroundMusic() => _soundEffect.Volume *= 0.5f;

    public void RestoreBackgroundMusic() => _soundEffect.Volume *= 2f;
}