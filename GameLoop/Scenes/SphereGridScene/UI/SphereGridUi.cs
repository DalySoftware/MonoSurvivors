using System;
using System.Collections.Generic;
using System.Linq;
using GameLoop.Input;
using GameLoop.Rendering;
using GameLoop.UI;
using Gameplay.Audio;
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
    private readonly static Color IconLockedColor = ColorPalette.White.ShiftLightness(-0.10f);
    private readonly static Color IconColor = ColorPalette.White;
    private Node? _hoveredNode;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly RenderScaler _renderScaler;
    private readonly SphereGrid _grid;
    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly ToolTipRenderer _toolTipRenderer;
    private readonly SphereGridContent _content;
    private readonly FogOfWarMask _fog;
    private readonly GameInputState _inputState;
    private readonly IAudioPlayer _audio;

    private readonly Dictionary<Node, NodeColorCache> _colorCache = [];

    private readonly Panel _titlePanel;

    /// <summary>
    ///     UI overlay for the sphere grid levelling system
    /// </summary>
    public SphereGridUi(
        GraphicsDevice graphicsDevice,
        RenderScaler renderScaler,
        SphereGrid grid,
        PrimitiveRenderer primitiveRenderer,
        ToolTipRenderer toolTipRenderer,
        Panel.Factory panelFactory,
        SphereGridContent content,
        FogOfWarMask fog,
        NodePositionMap nodePositions,
        ISphereGridCamera camera,
        GameInputState inputState,
        IAudioPlayer audio)
    {
        _graphicsDevice = graphicsDevice;
        _renderScaler = renderScaler;
        _grid = grid;
        _primitiveRenderer = primitiveRenderer;
        _toolTipRenderer = toolTipRenderer;
        _content = content;
        _fog = fog;
        _inputState = inputState;
        _audio = audio;
        NodePositions = nodePositions.Positions;

        Camera = camera;
        FocusedNode = grid.Root;

        var panelSize = Panel.Factory.MeasureByInterior(TitleSize);
        var exterior = renderScaler.UiRectangle()
            .CreateAnchoredRectangle(UiAnchor.TopCenter, panelSize, new Vector2(0f, 50f));
        _titlePanel = panelFactory.DefineByExterior(exterior);

        // Ensure the fog is rendered to prevent flicker
        RebuildFog();
        BuildColorCache();
    }

    private Vector2 TitleSize => _content.FontLarge.MeasureString(TitleText(100));
    private InputMethod InputMethod => _inputState.CurrentInputMethod;

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

    private void BuildColorCache()
    {
        // This could actually be cached per category but doing it per node is completely sufficient

        _colorCache.Clear();

        foreach (var node in _grid.Nodes)
        {
            var baseColor = node.PowerUp.BaseColor().ShiftChroma(-0.1f);

            _colorCache[node] = new NodeColorCache
            {
                Locked = baseColor.WithChroma(0.05f),
                Unlockable = baseColor.ShiftLightness(0.05f).WithChroma(0.10f),
                Unlocked = baseColor.ShiftLightness(-0.04f),
            };
        }
    }

    private void UpdateCameraFollow()
    {
        if (HideFocus || FocusedNode is not { } node)
        {
            Camera.Target = null;
            return;
        }

        Camera.Target = new NodePositionWrapper(node, NodePositions);
    }

    private static string TitleText(int points) => $"You have {points} Skill Points to spend";

    internal void Update(GameTime gameTime)
    {
        RebuildFog();

        if (!HideFocus)
            Camera.Update(gameTime);
    }

    private void RebuildFog() => _fog.Rebuild(_grid.UnlockedNodes.Select(n => NodePositions[n]));

    internal bool IsVisible(Vector2 gridPosition) => _fog.IsVisible(gridPosition);

    internal void UpdateHoveredNode(Vector2? mouseScreenPos)
    {
        if (mouseScreenPos is not { } screenPosition)
        {
            _hoveredNode = null;
            return;
        }

        var mouseWorldPos = Camera.ScreenToWorld(screenPosition);

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

        Unlock(_hoveredNode);
    }

    internal void UnlockFocussedNode() => Unlock(FocusedNode);

    private void Unlock(Node node)
    {
        if (_grid.Unlock(node))
            _audio.Play(SoundEffectTypes.UnlockNode);
    }

    internal void Draw(SpriteBatch spriteBatch)
    {
        _graphicsDevice.Clear(ColorPalette.Charcoal);

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
                var color = isUnlocked ? ColorPalette.Yellow : ColorPalette.Gray;
                _primitiveRenderer.DrawLine(spriteBatch, nodePos, neighborPos, color, 8f, Layers.Edges);
            }
        }
    }

    private void DrawNodes(SpriteBatch spriteBatch)
    {
        foreach (var node in _grid.Nodes)
        {
            if (!NodePositions.TryGetValue(node, out var nodePos))
                continue;

            var isUnlocked = _grid.IsUnlocked(node);
            var canUnlock = _grid.CanUnlock(node);

            var colors = _colorCache[node];

            var nodeColor = isUnlocked ? colors.Unlocked :
                canUnlock ? colors.Unlockable :
                colors.Locked;

            var iconColor = !isUnlocked && !canUnlock ? IconLockedColor : IconColor;

            if (IsFocused(node) || IsHovered(node))
                nodeColor = nodeColor.ShiftLightness(0.4f); // small, unavoidable runtime tweak

            var texture = NodeTexture(node);
            DrawNode(spriteBatch, texture, nodePos, nodeColor);

            var iconTexture = _content.PowerUpIcons.IconFor(node.PowerUp);
            if (iconTexture != null)
                spriteBatch.Draw(
                    iconTexture,
                    nodePos,
                    origin: iconTexture.Centre,
                    color: iconColor,
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
        _titlePanel.Draw(spriteBatch, ColorPalette.Peach, ColorPalette.DarkGray.ShiftLightness(-0.1f));

        var titleText = TitleText(_grid.AvailablePoints);
        var titleSize = _content.FontLarge.MeasureString(titleText);
        var titlePosition = _titlePanel.Interior.CreateAnchoredRectangle(UiAnchor.Centre, titleSize).TopLeft;
        spriteBatch.DrawString(_content.FontLarge, titleText, titlePosition, ColorPalette.White,
            layerDepth: _titlePanel.InteriorLayerDepth + 0.01f);

        var helpText = HelpText();
        var helpSize = _content.FontMedium.MeasureString(helpText);
        var helpRectangle = _renderScaler.UiRectangle()
            .CreateAnchoredRectangle(UiAnchor.BottomCenter, helpSize, new Vector2(0f, -50f));
        spriteBatch.DrawString(_content.FontMedium, helpText, helpRectangle.TopLeft, ColorPalette.LightGray,
            layerDepth: Layers.HelpText);

        DrawTooltips(spriteBatch);
        _fog.Draw(spriteBatch);
    }

    private string HelpText() => InputMethod switch
    {
        InputMethod.KeyboardMouse =>
            "Click nodes to unlock | Middle-click drag to move | SPACE to close | T to recenter",
        InputMethod.Gamepad => "[A] to unlock | [Y] to close",
        _ => throw new ArgumentOutOfRangeException(nameof(InputMethod)),
    };

    private Texture2D NodeTexture(Node node) => node.Rarity switch
    {
        NodeRarity.Legendary => _content.GridNodeLarge,
        NodeRarity.Rare => _content.GridNodeMedium,
        _ => _content.GridNodeSmall,
    };

    private static void DrawNode(SpriteBatch spriteBatch, Texture2D sprite, Vector2 center, Color color) =>
        spriteBatch.Draw(sprite, center, origin: sprite.Centre, color: color, layerDepth: Layers.Nodes);

    private void DrawTooltip(SpriteBatch spriteBatch, Node node, bool drawAtNode = false)
    {
        var tooltip = GetTooltip(node);
        var layer = _titlePanel.InteriorLayerDepth + 0.05f;

        if (drawAtNode)
            _toolTipRenderer.DrawTooltipAt(spriteBatch, tooltip,
                Camera.WorldToScreen(NodePositions[node]) + new Vector2(40f, 40f), layer);
        else
            _toolTipRenderer.DrawTooltipAtMouse(spriteBatch, tooltip, layer);
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
            UnlockLineFor(node),
        ];

        return new ToolTip(title, body);
    }

    private ToolTipBodyLine UnlockLineFor(Node node) =>
        _grid.IsUnlocked(node) ? new ToolTipBodyLine("[Unlocked]", ColorPalette.Green) :
        _grid.CanUnlock(node) ? new ToolTipBodyLine(UnlockText(), ColorPalette.Cyan) :
        new ToolTipBodyLine("[Cannot unlock]", ColorPalette.DarkGray);

    private string UnlockText() =>
        InputMethod is InputMethod.KeyboardMouse ? "[Click to unlock]" : "[Press A to unlock]";

    private sealed class NodeColorCache
    {
        public Color Locked;
        public Color Unlockable;
        public Color Unlocked;
    }
}