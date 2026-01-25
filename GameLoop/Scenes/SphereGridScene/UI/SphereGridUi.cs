using System;
using System.Collections.Generic;
using System.Linq;
using GameLoop.Input;
using GameLoop.Rendering;
using GameLoop.UI;
using Gameplay.Audio;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Levelling.SphereGrid.UI;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.Tooltips;
using Gameplay.Telemetry;
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
    private readonly PerformanceMetrics _perf;

    private List<Node> _nodesOrdered = [];
    private Dictionary<Node, int> _nodeIndex = [];
    private List<(Node A, Node B)> _uniqueEdges = [];

    private EdgeDrawData[] _edgeDrawCache = Array.Empty<EdgeDrawData>();

    // Draw buckets
    private readonly List<Node> _smallNodes = new(256);
    private readonly List<Node> _mediumNodes = new(256);
    private readonly List<Node> _largeNodes = new(256);
    private readonly List<(SpriteFrame frame, Node node)> _weaponIcons = new(256);
    private readonly List<(SpriteFrame frame, Node node)> _powerUpIcons = new(256);

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
        IAudioPlayer audio,
        PerformanceMetrics perf)
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
        _perf = perf;
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
        BuildDrawLists();
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

    private void BuildDrawLists()
    {
        // Stable iteration order (HashSet order is not)
        _nodesOrdered = _grid.Nodes
            .OrderBy(n => NodePositions[n].Y)
            .ThenBy(n => NodePositions[n].X)
            .ToList();

        _nodeIndex = new Dictionary<Node, int>(_nodesOrdered.Count);
        for (var i = 0; i < _nodesOrdered.Count; i++)
            _nodeIndex[_nodesOrdered[i]] = i;

        // Unique edges (avoid drawing both A->B and B->A)
        _uniqueEdges = new List<(Node, Node)>(_nodesOrdered.Count * 3);
        foreach (var node in _nodesOrdered)
        foreach (var neighbor in node.Neighbours.Values)
        {
            if (!_nodeIndex.TryGetValue(neighbor, out var ni)) continue;
            if (_nodeIndex[node] < ni)
                _uniqueEdges.Add((node, neighbor));
        }

        // Node buckets by base texture (only 3 textures)
        _smallNodes.Clear();
        _mediumNodes.Clear();
        _largeNodes.Clear();

        // Icon buckets by texture (minimize texture binds)
        _weaponIcons.Clear();
        _powerUpIcons.Clear();

        foreach (var node in _nodesOrdered)
        {
            switch (node.Rarity)
            {
                case NodeRarity.Legendary: _largeNodes.Add(node); break;
                case NodeRarity.Rare: _mediumNodes.Add(node); break;
                default: _smallNodes.Add(node); break;
            }

            var frame = _content.PowerUpIcons.IconFor(node.PowerUp);
            if (frame is null) continue;

            if (node.PowerUp is WeaponUnlock)
                _weaponIcons.Add((frame.Value, node));
            else
                _powerUpIcons.Add((frame.Value, node));
        }

        BuildEdgeDrawCache();
    }

    private void BuildEdgeDrawCache()
    {
        // Geometry is static as long as NodePositions doesn't change
        var cache = new EdgeDrawData[_uniqueEdges.Count];

        for (var i = 0; i < _uniqueEdges.Count; i++)
        {
            var (a, b) = _uniqueEdges[i];

            // If NodePositions is guaranteed complete (it should be), use indexer (fast path).
            var aPos = NodePositions[a];
            var bPos = NodePositions[b];

            var d = bPos - aPos;
            var length = d.Length(); // sqrt once, not every frame
            var rotation = MathF.Atan2(d.Y, d.X);

            const float thickness = 8f;
            cache[i] = new EdgeDrawData(a, b, aPos, rotation, new Vector2(length, thickness));
        }

        _edgeDrawCache = cache;
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

        using (var _ = _perf.MeasureProbe("World"))
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp,
                sortMode: SpriteSortMode.Deferred,
                transformMatrix: Camera.Transform);

            DrawEdges(spriteBatch); // uses _uniqueEdges
            DrawNodeBackplates(spriteBatch); // only the 3 node textures
            DrawIcons(spriteBatch); // grouped by icon texture

            spriteBatch.End();
        }


        // Screen space batch - Will be layered on top of the world space batch 
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Deferred);
        DrawScreenspaceUi(spriteBatch);
        spriteBatch.End();
    }

    private void DrawEdges(SpriteBatch spriteBatch)
    {
        for (var i = 0; i < _edgeDrawCache.Length; i++)
        {
            ref readonly var e = ref _edgeDrawCache[i];

            var isUnlocked = _grid.IsUnlocked(e.A) && _grid.IsUnlocked(e.B);
            var color = isUnlocked ? ColorPalette.Yellow : ColorPalette.Gray;

            _primitiveRenderer.DrawLine(spriteBatch, e.Start, e.Rotation, e.Scale, color, Layers.Edges);
        }
    }

    private void DrawNodeBackplates(SpriteBatch spriteBatch)
    {
        DrawNodeBackplates(spriteBatch, _content.GridNodeSmall, _smallNodes);
        DrawNodeBackplates(spriteBatch, _content.GridNodeMedium, _mediumNodes);
        DrawNodeBackplates(spriteBatch, _content.GridNodeLarge, _largeNodes);
    }

    private void DrawNodeBackplates(SpriteBatch spriteBatch, Texture2D texture, List<Node> nodes)
    {
        var origin = texture.Centre;
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (!NodePositions.TryGetValue(node, out var pos)) continue;

            var isUnlocked = _grid.IsUnlocked(node);
            var canUnlock = _grid.CanUnlock(node);
            var colors = _colorCache[node];

            var nodeColor = isUnlocked ? colors.Unlocked :
                canUnlock ? colors.Unlockable :
                colors.Locked;

            if (IsFocused(node) || IsHovered(node))
                nodeColor = nodeColor.ShiftLightness(0.4f);

            spriteBatch.Draw(texture, pos, origin: origin, color: nodeColor);
        }
    }

    private void DrawIcons(SpriteBatch spriteBatch)
    {
        DrawIconList(spriteBatch, _powerUpIcons);
        DrawIconList(spriteBatch, _weaponIcons);
    }

    private void DrawIconList(SpriteBatch spriteBatch, List<(SpriteFrame frame, Node node)> icons)
    {
        for (var i = 0; i < icons.Count; i++)
        {
            var (icon, node) = icons[i];
            if (!NodePositions.TryGetValue(node, out var pos)) continue;

            var isUnlocked = _grid.IsUnlocked(node);
            var canUnlock = _grid.CanUnlock(node);
            var iconColor = !isUnlocked && !canUnlock ? IconLockedColor : IconColor;

            spriteBatch.Draw(icon.Texture, pos, iconColor, icon.Source, origin: icon.Origin);
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
        _fog.Draw(spriteBatch);

        var helpText = HelpText();
        var helpSize = _content.FontMedium.MeasureString(helpText);
        var helpRectangle = _renderScaler.UiRectangle()
            .CreateAnchoredRectangle(UiAnchor.BottomCenter, helpSize, new Vector2(0f, -50f));
        spriteBatch.DrawString(_content.FontMedium, helpText, helpRectangle.TopLeft, ColorPalette.LightGray);

        DrawTooltips(spriteBatch);

        _titlePanel.Draw(spriteBatch, ColorPalette.Peach, ColorPalette.DarkGray.ShiftLightness(-0.1f));

        var titleText = TitleText(_grid.AvailablePoints);
        var titleSize = _content.FontLarge.MeasureString(titleText);
        var titlePosition = _titlePanel.Interior.CreateAnchoredRectangle(UiAnchor.Centre, titleSize).TopLeft;
        spriteBatch.DrawString(_content.FontLarge, titleText, titlePosition, ColorPalette.White);
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

    private void DrawTooltip(SpriteBatch spriteBatch, Node node, bool drawAtNode = false)
    {
        var tooltip = GetTooltip(node);
        if (drawAtNode)
            _toolTipRenderer.DrawTooltipAt(spriteBatch, tooltip,
                Camera.WorldToScreen(NodePositions[node]) + new Vector2(40f, 40f));
        else
            _toolTipRenderer.DrawTooltipAtMouse(spriteBatch, tooltip);
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

    private readonly struct EdgeDrawData
    {
        public readonly Node A;
        public readonly Node B;
        public readonly Vector2 Start;
        public readonly float Rotation;
        public readonly Vector2 Scale; // (length, thickness)

        public EdgeDrawData(Node a, Node b, Vector2 start, float rotation, Vector2 scale)
        {
            A = a;
            B = b;
            Start = start;
            Rotation = rotation;
            Scale = scale;
        }
    }

    private sealed class NodeColorCache
    {
        public Color Locked;
        public Color Unlockable;
        public Color Unlocked;
    }
}