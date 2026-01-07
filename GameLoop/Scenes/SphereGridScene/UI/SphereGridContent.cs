using ContentLibrary;
using Gameplay.Levelling.PowerUps;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.SphereGridScene.UI;

internal class SphereGridContent(ContentManager content, PowerUpIcons powerUpIcons)
{
    internal SpriteFont FontLarge { get; } = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);
    internal SpriteFont FontMedium { get; } = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Medium);
    internal Texture2D GridNodeLarge { get; } = content.Load<Texture2D>(Paths.Images.GridNode.Large);
    internal Texture2D GridNodeMedium { get; } = content.Load<Texture2D>(Paths.Images.GridNode.Medium);
    internal Texture2D GridNodeSmall { get; } = content.Load<Texture2D>(Paths.Images.GridNode.Small);
    internal PowerUpIcons PowerUpIcons { get; } = powerUpIcons;
}