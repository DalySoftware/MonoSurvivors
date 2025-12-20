using System;
using System.Collections.Generic;
using System.Linq;
using GameLoop.UI;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.SphereGridScene.UI;

/// <summary>
///     UI overlay for the sphere grid levelling system
/// </summary>
internal class SphereGridUi
{
    private readonly FogOfWarMask _fog;

    private Node? _hoveredNode;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SphereGrid _grid;
    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly ToolTipRenderer _toolTipRenderer;
    private readonly PanelRenderer _panelRenderer;
    private readonly SphereGridContent _content;
    /// <summary>
    ///     UI overlay for the sphere grid levelling system
    /// </summary>
    public SphereGridUi(
        GraphicsDevice graphicsDevice,
        SphereGrid grid,
        PrimitiveRenderer primitiveRenderer,
        ToolTipRenderer toolTipRenderer,
        PanelRenderer panelRenderer,
        SphereGridContent content,
        FogOfWarMask fog)
    {
        _graphicsDevice = graphicsDevice;
        _grid = grid;
        _primitiveRenderer = primitiveRenderer;
        _toolTipRenderer = toolTipRenderer;
        _panelRenderer = panelRenderer;
        _content = content;

        _fog = fog;

        ScreenSpaceOrigin = graphicsDevice.Viewport.Bounds.Center.ToVector2();
        NodePositions = new SphereGridPositioner(grid.Root).NodePositions();
        FocusedNode = grid.Root;
    }

    private Panel TitlePanel => _panelRenderer.Define(TitleCentre, TitleSize);
    private Vector2 TitleSize => _content.FontLarge.MeasureString(TitleText(100));
    private Vector2 TitleCentre => new(_graphicsDevice.Viewport.Width / 2f, 80);

    internal Vector2 ScreenSpaceOrigin { get; set; }

    internal IReadOnlyDictionary<Node, Vector2> NodePositions { get; }

    internal Node FocusedNode { get; set; }
    internal bool HideFocus { get; set; } = false;

    private static string TitleText(int points) => $"You have {points} Skill Points to spend";

    internal void Update() => _fog.Rebuild(
        _grid.UnlockedNodes.Select(n => ScreenSpaceOrigin + NodePositions[n]));

    internal bool IsVisible(Vector2 gridPosition)
    {
        var screenPosition = ScreenSpaceOrigin + gridPosition;
        return _fog.IsVisible(screenPosition);
    }

    internal void UpdateHoveredNode(Vector2 mousePos)
    {
        if (!_fog.IsVisible(mousePos))
        {
            _hoveredNode = null;
            return;
        }

        _hoveredNode = NodePositions.FirstOrDefault(kvp =>
        {
            var screenPos = ScreenSpaceOrigin + kvp.Value;
            var radius = NodeTexture(kvp.Key).Width * 0.5f;
            return Vector2.Distance(mousePos, screenPos) <= radius;
        }).Key;
    }

    internal void UnlockHoveredNode()
    {
        if (_hoveredNode == null) return;

        _grid.Unlock(_hoveredNode);
    }

    internal void UnlockFocussedNode() => _grid.Unlock(FocusedNode);

    internal void Draw(SpriteBatch spriteBatch)
    {
        _graphicsDevice.Clear(Color.DarkSlateGray);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

        var viewport = _graphicsDevice.Viewport;

        TitlePanel.Draw(spriteBatch, Color.White, Color.SlateGray.ShiftLightness(-.1f));

        var titleCenter = TitlePanel.Centre;
        var titleText = TitleText(_grid.AvailablePoints);
        var titleSize = _content.FontLarge.MeasureString(titleText);
        spriteBatch.DrawString(_content.FontLarge, titleText, titleCenter, Color.White, origin: titleSize / 2f,
            layerDepth: TitlePanel.InteriorLayerDepth + 0.01f);

        const string helpText = "Click nodes to unlock | Tab to close";
        var helpSize = _content.FontMedium.MeasureString(helpText);
        spriteBatch.DrawString(_content.FontMedium, helpText,
            new Vector2(viewport.Width / 2f - helpSize.X / 2, viewport.Height - 40),
            Color.Gray, layerDepth: Layers.HelpText);

        foreach (var node in _grid.Nodes)
        {
            if (!NodePositions.TryGetValue(node, out var nodePos)) continue;

            var screenNodePos = ScreenSpaceOrigin + nodePos;

            foreach (var (_, neighbor) in node.Neighbours)
            {
                if (!NodePositions.TryGetValue(neighbor, out var neighborPos)) continue;

                var screenNeighborPos = ScreenSpaceOrigin + neighborPos;
                var isUnlocked = _grid.IsUnlocked(node) && _grid.IsUnlocked(neighbor);

                var color = isUnlocked ? Color.Gold : Color.Gray * 0.5f;
                _primitiveRenderer.DrawLine(spriteBatch, screenNodePos, screenNeighborPos, color, 8f, Layers.Edges);
            }
        }

        foreach (var node in _grid.Nodes)
            DrawNode(spriteBatch, node);

        _fog.Draw(spriteBatch);
        spriteBatch.End();
    }

    private void DrawNode(SpriteBatch spriteBatch, Node node)
    {
        if (!NodePositions.TryGetValue(node, out var nodePos)) return;

        var isFocussed = !HideFocus && node == FocusedNode;
        var isHovered = node == _hoveredNode;
        var isUnlocked = _grid.IsUnlocked(node);
        var canUnlock = _grid.CanUnlock(node);

        var baseColor = node.PowerUp.BaseColor();
        var color =
            isFocussed || isHovered ? baseColor.ShiftHue(MathF.PI) :
            isUnlocked ? baseColor.ShiftLightness(-0.25f) :
            canUnlock ? baseColor.ShiftChroma(-0f).ShiftLightness(0.3f) :
            baseColor.ShiftChroma(-0.12f);

        var screenNodePos = ScreenSpaceOrigin + nodePos;
        var texture = NodeTexture(node);
        DrawNode(spriteBatch, texture, screenNodePos, color);

        var iconTexture = _content.PowerUpIcons.IconFor(node);
        if (iconTexture != null)
            spriteBatch.Draw(iconTexture, screenNodePos, origin: iconTexture.Centre, color: color,
                layerDepth: Layers.Nodes + 0.01f);

        if (isHovered) DrawTooltip(spriteBatch, node);
        if (isFocussed) DrawTooltip(spriteBatch, node, true);
    }

    private Texture2D NodeTexture(Node node) => node.Rarity switch
    {
        NodeRarity.Legendary => _content.GridNodeLarge,
        NodeRarity.Rare => _content.GridNodeMedium,
        _ => _content.GridNodeSmall,
    };

    private void DrawNode(SpriteBatch spriteBatch, Texture2D sprite, Vector2 center, Color color) =>
        spriteBatch.Draw(sprite, center, origin: sprite.Centre, color: color, layerDepth: Layers.Nodes);

    private void DrawTooltip(SpriteBatch spriteBatch, Node node, bool drawAtNode = false)
    {
        var tooltip = GetTooltip(node);

        if (drawAtNode)
            _toolTipRenderer.DrawTooltipAt(spriteBatch, tooltip,
                ScreenSpaceOrigin + NodePositions[node] + new Vector2(40f, 40f));
        else
            _toolTipRenderer.DrawTooltip(spriteBatch, tooltip);
    }

    private ToolTip GetTooltip(Node node)
    {
        if (node.PowerUp is not { } powerUp)
            return new ToolTip("Starting Point", [
                new ToolTipBodyLine("Unlock connected nodes to power up!"),
            ]);

        var title = powerUp.Title();

        ToolTipBodyLine[] body =
        [
            new(powerUp.Description()),
            new($"Cost: {node.Cost} SP"),
            UnlockTextFor(node),
        ];

        return new ToolTip(title, body);
    }


    private ToolTipBodyLine UnlockTextFor(Node node) =>
        _grid.IsUnlocked(node) ? new ToolTipBodyLine("[Unlocked]", Color.LawnGreen) :
        _grid.CanUnlock(node) ? new ToolTipBodyLine("[Click to unlock]", Color.Turquoise) :
        new ToolTipBodyLine("[Cannot unlock]", Color.DimGray);
}