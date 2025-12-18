using GameLoop.Scenes.SphereGridScene.UI;
using GameLoop.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridScene(
    SpriteBatch spriteBatch,
    SphereGridUi sphereGridUi,
    SphereGridInputManager inputManager)
    : IScene
{
    public void Update(GameTime gameTime)
    {
        inputManager.Update();
        sphereGridUi.Update();
    }

    public void Draw(GameTime gameTime) => sphereGridUi.Draw(spriteBatch);
    public void Dispose() { }
}