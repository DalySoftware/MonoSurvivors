using System.Collections.Generic;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering;

public class OutlineRenderer(GraphicsDevice graphics)
{
    private readonly Dictionary<Texture2D, Texture2D> _silhouetteCache = new();
    private readonly Vector2[] _outlineVectors =
    [
        new(0, -4),
        new(0, 4),
        new(-4, 0),
        new(4, 0),

        new(-4, -4),
        new(-4, 4),
        new(4, -4),
        new(4, 4),
    ];

    public void DrawOutline(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle sourceRectangle,
        Vector2 origin, float layerDepth, Color color, Vector2 scale)
    {
        var silhouetteTexture = GetSilhouetteTexture(texture);

        foreach (var outline in _outlineVectors)
            spriteBatch.Draw(silhouetteTexture, position + outline, sourceRectangle: sourceRectangle,
                origin: origin, layerDepth: layerDepth, color: color, scale: scale);
    }

    private Texture2D GetSilhouetteTexture(Texture2D texture)
    {
        var silhouetteTexture = _silhouetteCache.TryGetValue(texture, out var cached)
            ? cached
            : _silhouetteCache[texture] = CreateSilhouette(texture);
        return silhouetteTexture;
    }

    private Texture2D CreateSilhouette(Texture2D source)
    {
        var data = new Color[source.Width * source.Height];
        source.GetData(data);

        for (var i = 0; i < data.Length; i++)
            if (data[i].A > 0)
                data[i] = ColorPalette.White;
            else
                data[i] = Color.Transparent;

        var silhouette = new Texture2D(graphics, source.Width, source.Height);
        silhouette.SetData(data);
        return silhouette;
    }
}