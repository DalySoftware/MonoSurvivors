using ContentLibrary;
using GameLoop.UI;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay;

internal class MainGameScene(
    SpriteBatch spriteBatch,
    EntityManager entityManager,
    EffectManager effectManager,
    EnemySpawner spawner,
    ChaseCamera camera,
    ContentManager content,
    ExperienceBar experienceBar,
    EntityRenderer entityRenderer,
    GameplayInputManager input,
    HealthBar healthBar)
    : IScene
{
    private readonly Texture2D _backgroundTile = content.Load<Texture2D>(Paths.Images.BackgroundTile);

    public void Dispose() { }

    public void Update(GameTime gameTime)
    {
        input.Update();
        effectManager.Update(gameTime);
        entityManager.Update(gameTime);
        camera.Follow(gameTime);
        spawner.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        DrawBackground();

        entityRenderer.Draw(entityManager.Entities);
        healthBar.Draw(spriteBatch);
        experienceBar.Draw(spriteBatch, Color.CadetBlue, Color.GreenYellow);
    }

    private void DrawBackground()
    {
        spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: camera.Transform,
            sortMode: SpriteSortMode.FrontToBack);
        spriteBatch.Draw(_backgroundTile, camera.VisibleWorldBounds, camera.VisibleWorldBounds,
            Color.DarkSlateGray.ShiftChroma(-0.02f).ShiftLightness(0.05f));
        spriteBatch.End();
    }
}