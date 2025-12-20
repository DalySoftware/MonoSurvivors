using System;
using System.Collections.Generic;
using System.Linq;
using GameLoop.UI;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Levelling.SphereGrid.UI;
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
    private Node? _hoveredNode;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SphereGrid _grid;
    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly ToolTipRenderer _toolTipRenderer;
    private readonly PanelRenderer _panelRenderer;
    private readonly SphereGridContent _content;
    private readonly FogOfWarMask _fog;

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
        FogOfWarMask fog,
        NodePositionMap nodePositions,
        ISphereGridCamera camera)
    {
        _graphicsDevice = graphicsDevice;
        _grid = grid;
        _primitiveRenderer = primitiveRenderer;
        _toolTipRenderer = toolTipRenderer;
        _panelRenderer = panelRenderer;
        _content = content;
        _fog = fog;
        NodePositions = nodePositions.Positions;

        Camera = camera;
        FocusedNode = grid.Root;
    }

    private Panel TitlePanel => _panelRenderer.Define(TitleCentre, TitleSize);
    private Vector2 TitleSize => _content.FontLarge.MeasureString(TitleText(100));
    private Vector2 TitleCentre => new(_graphicsDevice.Viewport.Width / 2f, 80);

    internal ISphereGridCamera Camera { get; }
    internal IReadOnlyDictionary<Node, Vector2> NodePositions { get; }

    internal Node FocusedNode
    {
        get;
        set
        {
            field = value;
            UpdateCameraFollow();
        }
    }

    internal bool HideFocus
    {
        get;
        set
        {
            field = value;
            UpdateCameraFollow();
        }
    } = false;

    private void UpdateCameraFollow()
    {
        if (HideFocus || FocusedNode is not { } node || NodePositions is not { } positions)
        {
            Camera.Target = null;
            return;
        }

        Camera.Target = new NodePositionWrapper(node, positions);
    }

    private static string TitleText(int points) => $"You have {points} Skill Points to spend";

    internal void Update(GameTime gameTime)
    {
        _fog.Rebuild(
            _grid.UnlockedNodes.Select(n => NodePositions[n]));

        if (!HideFocus)
            Camera.Update(gameTime);
    }

    internal bool IsVisible(Vector2 gridPosition) => _fog.IsVisible(gridPosition);

    internal void UpdateHoveredNode(Vector2 mouseScreenPos)
    {
        var mouseWorldPos = Camera.ScreenToWorld(mouseScreenPos);

        if (!_fog.IsVisible(mouseWorldPos))
        {
            _hoveredNode = null;
            return;
        }

        _hoveredNode = NodePositions.FirstOrDefault(kvp =>
        {
            var radius = NodeTexture(kvp.Key).Width * 0.5f;
            return Vector2.Distance(mouseWorldPos, kvp.Value) <= radius;
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

        // World space batch
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack,
            transformMatrix: Camera.Transform);
        DrawEdges(spriteBatch);
        DrawNodes(spriteBatch);
        spriteBatch.End();

        // Screen space batch - Will be layered on top of the world space batch 
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);
        DrawScreenspaceUi(spriteBatch);
        spriteBatch.End();
    }

    private void DrawEdges(SpriteBatch spriteBatch)
    {
        foreach (var node in _grid.Nodes)
        {
            if (!NodePositions.TryGetValue(node, out var nodePos)) continue;

            foreach (var (_, neighbor) in node.Neighbours)
            {
                if (!NodePositions.TryGetValue(neighbor, out var neighborPos)) continue;

                var isUnlocked = _grid.IsUnlocked(node) && _grid.IsUnlocked(neighbor);
                var color = isUnlocked ? Color.Gold : Color.Gray * 0.5f;
                _primitiveRenderer.DrawLine(spriteBatch, nodePos, neighborPos, color, 8f, Layers.Edges);
            }
        }
    }

    private void DrawNodes(SpriteBatch spriteBatch)
    {
        foreach (var node in _grid.Nodes)
        {
            if (!NodePositions.TryGetValue(node, out var nodePos)) continue;

            var isFocussed = IsFocused(node);
            var isHovered = IsHovered(node);
            var isUnlocked = _grid.IsUnlocked(node);
            var canUnlock = _grid.CanUnlock(node);

            var baseColor = node.PowerUp.BaseColor();
            var color =
                isFocussed || isHovered ? baseColor.ShiftHue(MathF.PI) :
                isUnlocked ? baseColor.ShiftLightness(-0.25f) :
                canUnlock ? baseColor.ShiftChroma(-0f).ShiftLightness(0.3f) :
                baseColor.ShiftChroma(-0.12f);

            var texture = NodeTexture(node);
            DrawNode(spriteBatch, texture, nodePos, color);

            var iconTexture = _content.PowerUpIcons.IconFor(node);
            if (iconTexture != null)
                spriteBatch.Draw(iconTexture, nodePos, origin: iconTexture.Centre, color: color,
                    layerDepth: Layers.Nodes + 0.01f);
        }
    }

    private void DrawTooltips(SpriteBatch spriteBatch)
    {
        foreach (var node in _grid.Nodes.Where(n => IsFocused(n) || IsHovered(n)))
            DrawTooltip(spriteBatch, node, IsFocused(node));
    }

    private bool IsFocused(Node node) => !HideFocus && node == FocusedNode;
    private bool IsHovered(Node node) => node == _hoveredNode;

    private void DrawScreenspaceUi(SpriteBatch spriteBatch)
    {
        var viewport = _graphicsDevice.Viewport;

        TitlePanel.Draw(spriteBatch, Color.White, Color.SlateGray.ShiftLightness(-0.1f));

        var titleCenter = TitlePanel.Centre;
        var titleText = TitleText(_grid.AvailablePoints);
        var titleSize = _content.FontLarge.MeasureString(titleText);
        spriteBatch.DrawString(_content.FontLarge, titleText, titleCenter, Color.White,
            origin: titleSize / 2f, layerDepth: TitlePanel.InteriorLayerDepth + 0.01f);

        const string helpText = "Click nodes to unlock | Tab to close";
        var helpSize = _content.FontMedium.MeasureString(helpText);
        spriteBatch.DrawString(_content.FontMedium, helpText,
            new Vector2(viewport.Width / 2f - helpSize.X / 2, viewport.Height - 40),
            Color.Gray, layerDepth: Layers.HelpText);

        DrawTooltips(spriteBatch);
        _fog.Draw(spriteBatch);
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
                Camera.WorldToScreen(NodePositions[node]) + new Vector2(40f, 40f));
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