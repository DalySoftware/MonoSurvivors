using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

internal sealed class AsepriteAtlas
{
    private readonly Dictionary<string, SpriteFrame> _frames;

    internal AsepriteAtlas(Texture2D texture, string json)
    {
        var root = JsonSerializer.Deserialize<Root>(json, new JsonSerializerOptions
                   {
                       PropertyNameCaseInsensitive = true,
                   }) ??
                   throw new InvalidOperationException("Failed to parse atlas JSON.");

        var frames = new Dictionary<string, SpriteFrame>(root.Frames.Length, StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < root.Frames.Length; i++)
        {
            var f = root.Frames[i];
            var key = Path.GetFileNameWithoutExtension(f.Filename);

            var rect = new Rectangle(f.Frame.X, f.Frame.Y, f.Frame.W, f.Frame.H);
            frames[key] = new SpriteFrame(texture, rect);
        }

        _frames = frames;
    }

    internal SpriteFrame GetFrame(string key) => _frames[key];

    private record Root(FrameEntry[] Frames);

    private readonly record struct FrameEntry(string Filename, Rect Frame);

    private readonly record struct Rect(int X, int Y, int W, int H);
}

public readonly record struct SpriteFrame(Texture2D Texture, Rectangle Source)
{
    public Vector2 Origin { get; } = new(Source.Width * 0.5f, Source.Height * 0.5f);
}