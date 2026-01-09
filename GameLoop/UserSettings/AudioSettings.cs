using System;

namespace GameLoop.UserSettings;

public class AudioSettings
{
    public float MasterVolume
    {
        get;
        set => field = RoundVolume(value);
    } = 0.8f;

    public float MusicVolume
    {
        get;
        set => field = RoundVolume(value);
    } = 0.25f;

    public float SoundEffectVolume
    {
        get;
        set => field = RoundVolume(value);
    } = 0.8f;

    private static float RoundVolume(float value) => RoundToNearest(value, 0.05m);

    private static float RoundToNearest(float value, decimal increment)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(increment, 0m);

        var dValue = (decimal)value;
        var rounded = Math.Round(dValue / increment) * increment;

        return (float)rounded;
    }
}