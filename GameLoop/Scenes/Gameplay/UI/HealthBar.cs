using ContentLibrary;
using Gameplay.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay.UI;

internal class HealthBarFactory(ContentManager content, PlayerCharacter player)
{
    internal HealthBar Create()
    {
        var position = new Vector2(75f, 75f);
        return new HealthBar(content, player, position);
    }
}

/// <summary>
///     Health bar that displays health using heart sprites
/// </summary>
internal class HealthBar(ContentManager content, PlayerCharacter player, Vector2 position)
{
    private readonly Texture2D _heartEmpty = content.Load<Texture2D>(Paths.Images.Heart.Empty);
    private readonly Texture2D _heartFull = content.Load<Texture2D>(Paths.Images.Heart.Full);
    private readonly Texture2D _heartHalf = content.Load<Texture2D>(Paths.Images.Heart.Half);

    internal void Draw(SpriteBatch spriteBatch)
    {
        var currentHealth = player.Health;
        var maxHealth = player.Stats.MaxHealth;

        var totalHearts = (maxHealth + 1) / 2;
        var fullHearts = currentHealth / 2;
        var hasHalf = currentHealth % 2 != 0;

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        var i = 0;

        while (i < fullHearts) DrawHeart(spriteBatch, _heartFull, i++);
        if (hasHalf) DrawHeart(spriteBatch, _heartHalf, i++);
        while (i < totalHearts) DrawHeart(spriteBatch, _heartEmpty, i++);

        spriteBatch.End();
    }

    private void DrawHeart(SpriteBatch spriteBatch, Texture2D texture, int index)
    {
        var padding = texture.Height / 4;

        var offsetY = index * (texture.Height + padding);

        var drawPosition = position + new Vector2(0, offsetY);
        var origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

        spriteBatch.Draw(texture, drawPosition, null, Color.White, 0f, origin, 1f,
            SpriteEffects.None, 0f);
    }
}