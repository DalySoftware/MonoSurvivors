using System;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class AutoEndingSpriteBatch : IDisposable
{
    private readonly SpriteBatch _spriteBatch;
    public AutoEndingSpriteBatch(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
        _spriteBatch.Begin();
    }

    public void Dispose() => _spriteBatch.Dispose();
}

public static class SpriteBatchExtensions
{
    public static AutoEndingSpriteBatch BeginAndAutoEnd(this SpriteBatch spriteBatch) => new(spriteBatch);
}