using System;
using Gameplay;
using Microsoft.Xna.Framework;

namespace GameLoop.Scenes.Gameplay;

internal class PlayTime : IPlayTime
{
    public TimeSpan TimeSinceRunStart { get; private set; } = TimeSpan.Zero;

    internal void Update(GameTime gameTime) => TimeSinceRunStart += gameTime.ElapsedGameTime;
}