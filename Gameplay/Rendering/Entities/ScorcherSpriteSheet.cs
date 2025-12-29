using System;
using ContentLibrary;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class ScorcherSpriteSheet(ContentManager content) : ISpriteSheet
{
    private readonly static TimeSpan FrameDuration = TimeSpan.FromMilliseconds(100);
    private int _currentFrame;
    private TimeSpan _accumulator;

    public Texture2D Texture { get; } = content.Load<Texture2D>(Paths.Images.Sheets.Scorcher);

    public Rectangle GetFrameRectangle(IFrame _)
    {
        if (Texture == null) throw new InvalidOperationException("Texture not loaded");

        const int columns = 3;
        const int rows = 3;
        var cellWidth = Texture.Width / columns;
        var cellHeight = Texture.Height / rows;

        var column = _currentFrame % columns;
        var row = _currentFrame / columns;

        return new Rectangle(column * cellWidth, row * cellHeight, cellWidth, cellHeight);
    }

    public void Update(GameTime gameTime)
    {
        _accumulator += gameTime.ElapsedGameTime;

        while (_accumulator >= FrameDuration)
        {
            _accumulator -= FrameDuration;
            _currentFrame++;

            const int totalFrames = 8;
            if (_currentFrame >= totalFrames)
                _currentFrame = 0;
        }
    }

    internal record struct DummyFrame : IFrame;
}