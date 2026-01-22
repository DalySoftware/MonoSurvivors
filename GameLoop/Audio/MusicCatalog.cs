namespace GameLoop.Audio;

public static class MusicCatalog
{
    // Match the name exactly to the file name for now
    public enum Tracks
    {
        AppleStrudel,
        Venezuela,
    }

    public static string DesktopContentName(Tracks track) => $@"Music\{track}";
    public static string WebUrl(Tracks track) => $"music/{track}.wav";
}