using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.UI;

internal sealed class Panel : IUiElement
{
    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly Color _interiorColor;
    private readonly Color _frameColor;

    private Panel(
        NineSliceFrame nineSliceFrame,
        PrimitiveRenderer primitiveRenderer,
        UiRectangle exterior,
        float frameDepth,
        Color? interiorColor = null,
        Color? frameColor = null
    )
    {
        _primitiveRenderer = primitiveRenderer;
        Rectangle = exterior;
        _interiorColor = interiorColor ?? ColorPalette.Charcoal;
        _frameColor = frameColor ?? ColorPalette.Agave;

        Frame = nineSliceFrame.Define(Rectangle, frameDepth);
        InteriorLayerDepth = Frame.LayerDepth - 0.01f;
    }

    internal UiRectangle Interior => new(
        Rectangle.TopLeft + new Vector2(NineSliceFrame.CornerSize),
        Rectangle.Size - new Vector2(NineSliceFrame.CornerSize * 2)
    );

    internal Frame Frame { get; }
    internal float InteriorLayerDepth { get; }
    public UiRectangle Rectangle { get; }

    public void Draw(SpriteBatch spriteBatch) => Draw(spriteBatch, _frameColor, _interiorColor);

    internal void Draw(SpriteBatch spriteBatch, Color frameColor, Color interiorColor)
    {
        _primitiveRenderer.DrawRectangle(spriteBatch, Frame.InteriorRectangle, interiorColor, InteriorLayerDepth);

        foreach (var (pos, rotation) in Frame.CornerTriangles)
            _primitiveRenderer.DrawTriangle(spriteBatch, pos, interiorColor, rotation, InteriorLayerDepth);

        foreach (var rect in Frame.EdgeRectangles)
            _primitiveRenderer.DrawRectangle(spriteBatch, rect, interiorColor, InteriorLayerDepth);

        Frame.Draw(spriteBatch, frameColor);
    }

    internal sealed class Factory(ContentManager content, PrimitiveRenderer primitiveRenderer)
    {
        private readonly NineSliceFrame _frame = new(content);

        /// <summary>
        ///     Returns the required exterior size to fit an interior of the given size.
        /// </summary>
        internal static Vector2 MeasureByInterior(Vector2 interiorSize)
        {
            const int padding = NineSliceFrame.CornerSize * 2;
            return interiorSize + new Vector2(padding, padding);
        }

        /// <summary>
        ///     Define a panel by its final exterior rectangle (full size including borders).
        /// </summary>
        internal Panel DefineByExterior(UiRectangle exterior, float frameDepth = Layers.Ui + 0.1f)
            => new(_frame, primitiveRenderer, exterior, frameDepth);

        /// <summary>
        ///     Define a panel by desired interior rectangle; exterior will be auto-calculated.
        /// </summary>
        internal Panel DefineByInterior(UiRectangle interior, float frameDepth = Layers.Ui + 0.1f)
        {
            var exteriorSize = MeasureByInterior(interior.Size);

            // When wrapping an interior rectangle, preserve its (Origin, OriginAnchor).
            // Changing only Size expands/contracts the rectangle in the same placement frame.
            // Do NOT re-center using interior.Centre, or the entire layout will be re-anchored.
            var exterior = new UiRectangle(interior.Origin, exteriorSize, interior.OriginAnchor);

            return new Panel(_frame, primitiveRenderer, exterior, frameDepth);
        }
    }
}