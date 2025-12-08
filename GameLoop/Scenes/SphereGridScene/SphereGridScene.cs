using System;
using GameLoop.UI;
using Gameplay.Levelling;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridScene : IScene
{
    private readonly ContentManager _content;
    private readonly SpriteBatch _spriteBatch;
    private readonly SphereGridUi _sphereGridUi;
    private readonly SphereGridInputManager _input;

    public SphereGridScene(
        GraphicsDevice graphicsDevice,
        ContentManager coreContent,
        SphereGrid sphereGrid,
        PrimitiveRenderer primitiveRenderer,
        Action onClose)
    {
        _content = new ContentManager(coreContent.ServiceProvider)
        {
            RootDirectory = coreContent.RootDirectory
        };

        _spriteBatch = new SpriteBatch(graphicsDevice);

        
        _sphereGridUi = new SphereGridUi(_content, graphicsDevice, sphereGrid, primitiveRenderer);
        _input = new SphereGridInputManager
        {
            OnClose = onClose
        };
    }

    public void Update(GameTime gameTime)
    {
        // Don't update the background scene (game is paused)
        _input.Update();
        _sphereGridUi.Update();
    }

    public void Draw(GameTime gameTime)
    {
        // Just draw the sphere grid UI
        // (The dark overlay is drawn by the UI itself)
        _sphereGridUi.Draw(_spriteBatch);
    }

    public void Dispose()
    {
        _content.Dispose();
        _spriteBatch.Dispose();
    }
}
