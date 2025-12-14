using ContentLibrary;
using Gameplay.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

/// <summary>
///     Health bar that displays health using heart sprites
/// </summary>
internal class HealthBar(ContentManager content, PlayerCharacter player)
{
    private readonly Texture2D _heartEmpty = content.Load<Texture2D>(Paths.Images.Heart.Empty);
    private readonly Texture2D _heartFull = content.Load<Texture2D>(Paths.Images.Heart.Full);
    private readonly Texture2D _heartHalf = content.Load<Texture2D>(Paths.Images.Heart.Half);
    internal Vector2 Position { get; init; }

    internal void Draw(SpriteBatch spriteBatch)
    {
        var currentHealth = player.Health;
        var maxHealth = player.MaxHealth;

        // NB: integer maths
        var totalHearts = (maxHealth + 1) / 2;
        var fullHearts = currentHealth / 2;
        var hasHalf = currentHealth % 2 != 0;

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        var i = 0;

        // Full hearts
        while (i < fullHearts) DrawHeart(spriteBatch, _heartFull, i++);

        // Maybe half heart
        if (hasHalf) DrawHeart(spriteBatch, _heartHalf, i++);

        // Empty hearts
        while (i < totalHearts) DrawHeart(spriteBatch, _heartEmpty, i++);
        spriteBatch.End();
    }

    private void DrawHeart(SpriteBatch spriteBatch, Texture2D texture, int index)
    {
        var padding = texture.Width / 4;
        var x = index * (_heartFull.Width + padding);
        spriteBatch.Draw(texture, Position + new Vector2(x, 0), Color.White);
    }
}