using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

internal sealed class PanelRenderer(ContentManager content, PrimitiveRenderer primitiveRenderer)
{
    private readonly NineSliceFrame _frame = new(content);

    internal Panel Define(Vector2 centre, Vector2 interiorSize) =>
        new(_frame, primitiveRenderer, centre, interiorSize);
}

internal class Panel(
    NineSliceFrame nineSliceFrame,
    PrimitiveRenderer primitiveRenderer,
    Vector2 centre,
    Vector2 interiorSize)
{
    internal Vector2 Centre => centre;
    internal Frame Frame => nineSliceFrame.Define(centre, interiorSize, Layers.Ui + 0.01f);
    internal Vector2 ExteriorSize => new(Frame.ExteriorRectangle.Width, Frame.ExteriorRectangle.Height);

    internal void Draw(SpriteBatch spriteBatch, Color frameColor, Color interiorColor)
    {
        primitiveRenderer.DrawRectangle(spriteBatch, Frame.InteriorRectangle, interiorColor, Layers.Ui);

        foreach (var (pos, rotation) in Frame.CornerTriangles)
            primitiveRenderer.DrawTriangle(spriteBatch, pos, interiorColor, rotation, Layers.Ui);

        foreach (var rect in Frame.EdgeRectangles)
            primitiveRenderer.DrawRectangle(spriteBatch, rect, interiorColor, Layers.Ui);

        Frame.Draw(spriteBatch, frameColor);
    }
}