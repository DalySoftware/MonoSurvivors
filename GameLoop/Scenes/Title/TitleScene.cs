using ContentLibrary;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GameLoop.Scenes.Title;

internal class TitleScene(
    SpriteBatch spriteBatch,
    ContentManager content,
    TitleInputManager input)
    : IScene
{
    private readonly SpriteFont _titleFont = content.Load<SpriteFont>(Paths.Fonts.KarmaticArcade.Large);

    public void Update(GameTime gameTime) => input.Update();

    public void Draw(GameTime gameTime)
    {
        spriteBatch.Begin(SpriteSortMode.FrontToBack);

        var windowCentre = spriteBatch.GraphicsDevice.Viewport.Bounds.Center.ToVector2();
        var shadowOffset = new Vector2(5f, 5f);

        const float shadow = 0.4f;
        const float front = 0.6f;

        const string line1 = "Mono";
        var line1Centre = _titleFont.MeasureString(line1) / 2;
        var line1Position = windowCentre + new Vector2(0f, -75f);
        spriteBatch.DrawString(_titleFont, line1, line1Position + shadowOffset, Color.DimGray,
            origin: line1Centre, layerDepth: shadow);
        spriteBatch.DrawString(_titleFont, line1, line1Position, Color.DarkOrange, origin: line1Centre,
            layerDepth: front);

        const string line2 = "Survivors";
        var line2Centre = _titleFont.MeasureString(line2) / 2;
        var line2Position = windowCentre + new Vector2(0f, 75f);
        spriteBatch.DrawString(_titleFont, line2, line2Position + shadowOffset, Color.DimGray,
            origin: line2Centre, layerDepth: shadow);
        spriteBatch.DrawString(_titleFont, line2, line2Position, Color.DarkOrange, origin: line2Centre,
            layerDepth: front);

        spriteBatch.End();
    }

    public void Dispose() { }
}