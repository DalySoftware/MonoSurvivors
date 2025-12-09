using System;
using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering;
using Gameplay.Rendering.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.UI;

/// <summary>
///     UI overlay for the sphere grid levelling system
/// </summary>
public class SphereGridUi : UiElement
{
    private const float NodeSpacing = 40f;
    private float NodeRadius => _gridNodeSprite.Width / 2f;

    private readonly SphereGrid _grid;
    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly ToolTipRenderer _toolTipRenderer;
    private readonly SpriteFont _fontSmall;
    private readonly SpriteFont _fontLarge;
    private readonly Texture2D _gridNodeSprite;
    private readonly GraphicsDevice _graphicsDevice;

    private readonly Vector2 _offset;
    private Node? _hoveredNode;
    private MouseState _previousMouseState;
    private readonly IReadOnlyDictionary<Node, Vector2> _nodePositions;

    private static class Layers
    {
        internal const float Edges = 0.40f;
        internal const float Nodes = 0.50f;
        internal const float Title = 0.80f;
        internal const float ToolTip = 0.90f;
    }

    public SphereGridUi(ContentManager content, GraphicsDevice graphicsDevice, SphereGrid grid, PrimitiveRenderer primitiveRenderer)
    {
        _grid = grid;
        _primitiveRenderer = primitiveRenderer;
        _toolTipRenderer = new ToolTipRenderer(_primitiveRenderer, content);
        _graphicsDevice = graphicsDevice;
        _fontSmall = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Small);
        _fontLarge =  content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);
        _gridNodeSprite = content.Load<Texture2D>(Paths.Images.GridNode);

        // Center the grid on screen
        var viewport = _graphicsDevice.Viewport;
        _offset = new Vector2(viewport.Width, viewport.Height) / 2;

        _nodePositions = new SphereGridPositioner(_grid, NodeSpacing).NodePositions();
    }

    public void Update()
    {
        if (!IsVisible) return;

        var mouseState = Mouse.GetState();
        _hoveredNode = null;

        _hoveredNode = _grid.Nodes.FirstOrDefault(node =>
        {
            if (!_nodePositions.TryGetValue(node, out var nodePos)) return false;

            var screenPos = Position + _offset + nodePos;
            var mousePos = new Vector2(mouseState.X, mouseState.Y);

            return Vector2.Distance(mousePos, screenPos) <= NodeRadius;
        });

        // Click to unlock
        if (_hoveredNode != null  &&
            mouseState.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            _grid.Unlock(_hoveredNode);
        }

        _previousMouseState = mouseState;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _graphicsDevice.Clear(Color.DarkSlateGray);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

        var viewport = _graphicsDevice.Viewport;

        // Draw title
        const string title = "Level Up";
        var titleSize = _fontLarge.MeasureString(title);
        spriteBatch.DrawString(_fontLarge, title, new Vector2(viewport.Width / 2 - titleSize.X / 2, 20),
            Color.White, layerDepth: Layers.Title);

        const string helpText = "Click nodes to unlock | Tab to close";
        var helpSize = _fontSmall.MeasureString(helpText);
        spriteBatch.DrawString(_fontSmall, helpText,
            new Vector2(viewport.Width / 2 - helpSize.X / 2, viewport.Height - 40),
            Color.Gray, layerDepth: Layers.Title);

        // Draw connections first (so they're behind nodes)
        foreach (var node in _grid.Nodes)
        {
            if (!_nodePositions.TryGetValue(node, out var nodePos)) continue;

            var screenNodePos = Position + _offset + nodePos;

            foreach (var (_, neighbor) in node.Neighbours)
            {
                if (!_nodePositions.TryGetValue(neighbor, out var neighborPos)) continue;

                var screenNeighborPos = Position + _offset + neighborPos;
                var isUnlocked = _grid.IsUnlocked(node) && _grid.IsUnlocked(neighbor);

                var color = isUnlocked ? Color.Gold : Color.Gray * 0.5f;
                _primitiveRenderer.DrawLine(spriteBatch, screenNodePos, screenNeighborPos, color,2f, layerDepth: Layers.Edges);
            }
        }

        foreach (var node in _grid.Nodes)
            DrawNode(spriteBatch, node);

        spriteBatch.End();
    }

    private void DrawNode(SpriteBatch spriteBatch, Node node)
    {
        if (!_nodePositions.TryGetValue(node, out var nodePos)) return;

        var isUnlocked = _grid.IsUnlocked(node);
        var canUnlock = _grid.CanUnlock(node);

        var color =
            isUnlocked ? Color.Gold :
            canUnlock ? Color.Green :
            Color.DarkGray;

        var screenNodePos = Position + _offset + nodePos;
        DrawNode(spriteBatch, screenNodePos, color);

        if (node == _hoveredNode)
        {
            // Draw a highlight on top
            DrawNode(spriteBatch, screenNodePos, Color.White * 0.4f);
            DrawTooltip(spriteBatch, _hoveredNode);
        }
    }

    private void DrawNode(SpriteBatch spriteBatch, Vector2 center, Color color) => 
        spriteBatch.Draw(_gridNodeSprite, center, origin: _gridNodeSprite.Centre, color: color, layerDepth: Layers.Nodes);

    private void DrawTooltip(SpriteBatch spriteBatch, Node node)
    {

        if (node.PowerUp is not {} powerUp) return;

        var title = TitleFor(powerUp);
        
        ToolTipBodyLine[] body =
        [
            new(DescriptionFor(powerUp)),
            new($"Cost: {node.Cost} SP"),
            new(UnlockTextFor(node), Color.Gray),
        ];
        
        var tooltip = new ToolTip(title, body);
        _toolTipRenderer.DrawTooltip(spriteBatch, tooltip, Layers.ToolTip);
    }

    private static string TitleFor(IPowerUp powerUp) => powerUp switch
        {
            MaxHealthUp => "Increase Max Health",
            SpeedUp => "Increase Speed",
            DamageUp => "Increase Damage",
            _ => throw new ArgumentOutOfRangeException(nameof(powerUp)),
        };

    private static string DescriptionFor(IPowerUp powerUp) => powerUp switch
        {
            MaxHealthUp maxHealthUp => $"Increase Max Health by {(maxHealthUp.Value / 2).HeartLabel()}",
            SpeedUp speedUp => $"Increase Speed by {speedUp.Value:P1}",
            DamageUp damageUp => $"Increase Damage by {damageUp.Value:P1}",
            _ => throw new ArgumentOutOfRangeException(nameof(powerUp)),
        };
    
    private string UnlockTextFor(Node node) => 
        _grid.IsUnlocked(node) ? "[Unlocked]" :
        _grid.CanUnlock(node) ? "[Click to unlock]" : "[Cannot unlock]";
}

public static class Pluralization
{
    public static string HeartLabel(this int value) => $"{value} {(value == 1 ? "heart" : "hearts")}";
}

