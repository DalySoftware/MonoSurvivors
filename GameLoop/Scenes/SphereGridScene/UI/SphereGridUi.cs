using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using GameLoop.UI;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.SphereGridScene.UI;

/// <summary>
///     UI overlay for the sphere grid levelling system
/// </summary>
internal class SphereGridUi(
    ContentManager content,
    GraphicsDevice graphicsDevice,
    SphereGrid grid,
    PrimitiveRenderer primitiveRenderer,
    ToolTipRenderer toolTipRenderer,
    PanelRenderer panelRenderer)
{
    private readonly SpriteFont _fontLarge = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);
    private readonly SpriteFont _fontSmall = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Small);

    private readonly Texture2D _gridNodeLarge = content.Load<Texture2D>(Paths.Images.GridNode.Large);
    private readonly Texture2D _gridNodeMedium = content.Load<Texture2D>(Paths.Images.GridNode.Medium);
    private readonly Texture2D _gridNodeSmall = content.Load<Texture2D>(Paths.Images.GridNode.Small);
    private readonly PowerUpIcons _powerUpIcons = new(content);
    private readonly FogOfWarMask _fog = new(graphicsDevice, (int)(SphereGridPositioner.HexRadius * 1.5f), Layers.Fog);
    private readonly IReadOnlyDictionary<Node, Vector2> _nodePositions =
        new SphereGridPositioner(grid.Root).NodePositions();

    private Node? _hoveredNode;

    private Panel TitlePanel => panelRenderer.Define(TitleCentre, TitleSize);
    private Vector2 TitleSize => _fontLarge.MeasureString(TitleText(100));
    private Vector2 TitleCentre => new(graphicsDevice.Viewport.Width / 2f, 80);

    internal Vector2 ScreenSpaceOrigin { get; set; } = graphicsDevice.Viewport.Bounds.Center.ToVector2();

    private static string TitleText(int points) => $"You have {points} Skill Points to spend";

    internal void Update() => _fog.Rebuild(
        grid.UnlockedNodes.Select(n => ScreenSpaceOrigin + _nodePositions[n]));

    internal void UpdateHoveredNode(Vector2 mousePos) => _hoveredNode = _nodePositions.FirstOrDefault(kvp =>
    {
        var screenPos = ScreenSpaceOrigin + kvp.Value;

        if (!_fog.IsVisible(screenPos)) return false;

        var radius = NodeTexture(kvp.Key).Width * 0.5f;
        return Vector2.Distance(mousePos, screenPos) <= radius;
    }).Key;

    internal void UnlockHoveredNode()
    {
        if (_hoveredNode == null) return;

        grid.Unlock(_hoveredNode);
    }

    internal void Draw(SpriteBatch spriteBatch)
    {
        graphicsDevice.Clear(Color.DarkSlateGray);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

        var viewport = graphicsDevice.Viewport;

        TitlePanel.Draw(spriteBatch, Color.White, Color.SlateGray.ShiftLightness(-.1f));

        var titleCenter = TitlePanel.Centre;
        var titleText = TitleText(grid.AvailablePoints);
        var titleSize = _fontLarge.MeasureString(titleText);
        spriteBatch.DrawString(_fontLarge, titleText, titleCenter, Color.White, origin: titleSize / 2f,
            layerDepth: TitlePanel.InteriorLayerDepth + 0.01f);

        const string helpText = "Click nodes to unlock | Tab to close";
        var helpSize = _fontSmall.MeasureString(helpText);
        spriteBatch.DrawString(_fontSmall, helpText,
            new Vector2(viewport.Width / 2f - helpSize.X / 2, viewport.Height - 40),
            Color.Gray, layerDepth: Layers.HelpText);

        foreach (var node in grid.Nodes)
        {
            if (!_nodePositions.TryGetValue(node, out var nodePos)) continue;

            var screenNodePos = ScreenSpaceOrigin + nodePos;

            foreach (var (_, neighbor) in node.Neighbours)
            {
                if (!_nodePositions.TryGetValue(neighbor, out var neighborPos)) continue;

                var screenNeighborPos = ScreenSpaceOrigin + neighborPos;
                var isUnlocked = grid.IsUnlocked(node) && grid.IsUnlocked(neighbor);

                var color = isUnlocked ? Color.Gold : Color.Gray * 0.5f;
                primitiveRenderer.DrawLine(spriteBatch, screenNodePos, screenNeighborPos, color, 8f, Layers.Edges);
            }
        }

        foreach (var node in grid.Nodes)
            DrawNode(spriteBatch, node);

        _fog.Draw(spriteBatch);
        spriteBatch.End();
    }

    private void DrawNode(SpriteBatch spriteBatch, Node node)
    {
        if (!_nodePositions.TryGetValue(node, out var nodePos)) return;

        var isUnlocked = grid.IsUnlocked(node);
        var canUnlock = grid.CanUnlock(node);

        var baseColor = node.PowerUp.BaseColor();
        var color =
            isUnlocked ? baseColor.ShiftLightness(-0.25f) :
            canUnlock ? baseColor.ShiftChroma(-0f).ShiftLightness(0.3f) :
            baseColor.ShiftChroma(-0.12f);

        var screenNodePos = ScreenSpaceOrigin + nodePos;
        var texture = NodeTexture(node);
        DrawNode(spriteBatch, texture, screenNodePos, color);

        var iconTexture = _powerUpIcons.IconFor(node);
        if (iconTexture != null)
            spriteBatch.Draw(iconTexture, screenNodePos, origin: iconTexture.Centre, color: color,
                layerDepth: Layers.Nodes + 0.01f);

        if (node == _hoveredNode)
        {
            // Draw a highlight on top
            DrawNode(spriteBatch, texture, screenNodePos, Color.White * 0.4f);
            DrawTooltip(spriteBatch, _hoveredNode);
        }
    }

    private Texture2D NodeTexture(Node node) => node.Rarity switch
    {
        NodeRarity.Legendary => _gridNodeLarge,
        NodeRarity.Rare => _gridNodeMedium,
        _ => _gridNodeSmall,
    };

    private void DrawNode(SpriteBatch spriteBatch, Texture2D sprite, Vector2 center, Color color) =>
        spriteBatch.Draw(sprite, center, origin: sprite.Centre, color: color, layerDepth: Layers.Nodes);

    private void DrawTooltip(SpriteBatch spriteBatch, Node node)
    {
        if (node.PowerUp is not { } powerUp) return;

        var title = powerUp.Title();

        ToolTipBodyLine[] body =
        [
            new(powerUp.Description()),
            new($"Cost: {node.Cost} SP"),
            UnlockTextFor(node),
        ];

        var tooltip = new ToolTip(title, body);
        toolTipRenderer.DrawTooltip(spriteBatch, tooltip, Layers.ToolTip);
    }


    private ToolTipBodyLine UnlockTextFor(Node node) =>
        grid.IsUnlocked(node) ? new ToolTipBodyLine("[Unlocked]", Color.LawnGreen) :
        grid.CanUnlock(node) ? new ToolTipBodyLine("[Click to unlock]", Color.Turquoise) :
        new ToolTipBodyLine("[Cannot unlock]", Color.DimGray);

    private static class Layers
    {
        internal const float Edges = 0.40f;
        internal const float Nodes = 0.50f;
        internal const float Fog = 0.70f;
        internal const float HelpText = 0.8f;
        internal const float ToolTip = global::Gameplay.Rendering.Layers.Ui + 0.10f;
    }
}