using System;
using Microsoft.Xna.Framework;

namespace GameLoop.Scenes;

internal interface IScene : IDisposable
{
    void Update(GameTime gameTime);
    void Draw(GameTime gameTime);
}