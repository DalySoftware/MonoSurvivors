using System;
using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using Gameplay.Levelling;
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
    private const string PlaceholderName = "NAME";
    private const string PlaceholderDescription = "DESCRIPTION";
    private const float HexRadius = 40f;
    private const float NodeRadius = 8f;

    private readonly SphereGrid _grid;
    private readonly SpriteFont _font;
    private readonly GraphicsDevice _graphicsDevice;

    private Texture2D? _pixelTexture;
    private readonly Vector2 _offset;
    private Node? _hoveredNode;
    private MouseState _previousMouseState;
    private readonly Dictionary<Node, Vector2> _nodePositions = new();

    public SphereGridUi(ContentManager content, GraphicsDevice graphicsDevice, SphereGrid grid)
    {
        _grid = grid;
        _graphicsDevice = graphicsDevice;
        _font = content.Load<SpriteFont>(Paths.Fonts.TerminalGrotesqueOpen.Small);

        // Center the grid on screen
        var viewport = _graphicsDevice.Viewport;
        _offset = new Vector2(viewport.Width, viewport.Height) / 2;

        // Calculate hexagonal positions for all nodes
        CalculateNodePositions();
    }

    private void CalculateNodePositions()
    {
        _nodePositions.Clear();

        // Find a starting node (pick any node from the grid)
        var startNode = _grid.Nodes.FirstOrDefault();
        if (startNode == null) return;

        // Place start node at origin
        _nodePositions[startNode] = Vector2.Zero;

        // BFS to position all connected nodes
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();
        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentPos = _nodePositions[current];

            foreach (var (direction, neighbor) in current.Neighbours)
            {
                if (visited.Contains(neighbor)) continue;

                // Calculate hex offset based on direction
                var offset = GetHexOffset(direction);
                _nodePositions[neighbor] = currentPos + offset;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }
    }

    private static Vector2 GetHexOffset(EdgeDirection direction)
    {
        // Flat-top hexagon positioning
        // Using hex coordinate math where each direction has a specific offset
        var xStep = HexRadius * 1.5f;
        var yStep = HexRadius * (float)Math.Sqrt(3) / 2;

        return direction switch
        {
            EdgeDirection.TopLeft => new Vector2(-xStep, -yStep),
            EdgeDirection.TopRight => new Vector2(xStep, -yStep),
            EdgeDirection.MiddleLeft => new Vector2(-xStep * 2, 0),
            EdgeDirection.MiddleRight => new Vector2(xStep * 2, 0),
            EdgeDirection.BottomLeft => new Vector2(-xStep, yStep),
            EdgeDirection.BottomRight => new Vector2(xStep, yStep),
            _ => Vector2.Zero
        };
    }

    private Texture2D PixelTexture
    {
        get
        {
            if (_pixelTexture == null)
            {
                _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                _pixelTexture.SetData([Color.White]);
            }
            return _pixelTexture;
        }
    }

    public void Update()
    {
        if (!IsVisible) return;

        var mouseState = Mouse.GetState();
        _hoveredNode = null;

        // Find hovered node
        foreach (var node in _grid.Nodes)
        {
            if (!_nodePositions.TryGetValue(node, out var nodePos)) continue;

            var screenPos = Position + _offset + nodePos;
            var mousePos = new Vector2(mouseState.X, mouseState.Y);

            if (Vector2.Distance(mousePos, screenPos) <= NodeRadius)
            {
                _hoveredNode = node;

                // Click to unlock
                if (mouseState.LeftButton == ButtonState.Pressed &&
                    _previousMouseState.LeftButton == ButtonState.Released)
                {
                    _grid.Unlock(node);
                }
                break;
            }
        }

        _previousMouseState = mouseState;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _graphicsDevice.Clear(Color.DarkSlateGray);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        var viewport = _graphicsDevice.Viewport;

        // Draw title
        var title = "Sphere Grid";
        var titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2(viewport.Width / 2 - titleSize.X / 2, 20),
            Color.White);

        var helpText = "Click nodes to unlock | Tab to close";
        var helpSize = _font.MeasureString(helpText);
        spriteBatch.DrawString(_font, helpText,
            new Vector2(viewport.Width / 2 - helpSize.X / 2, viewport.Height - 40),
            Color.Gray);

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

                DrawLine(spriteBatch, screenNodePos, screenNeighborPos,
                    isUnlocked ? Color.Gold : Color.Gray * 0.5f,
                    2f);
            }
        }

        // Draw nodes
        foreach (var node in _grid.Nodes)
        {
            if (!_nodePositions.TryGetValue(node, out var nodePos)) continue;

            var screenNodePos = Position + _offset + nodePos;
            var isUnlocked = _grid.IsUnlocked(node);
            var canUnlock = _grid.CanUnlock(node);
            var isHovered = node == _hoveredNode;

            Color nodeColor;
            if (isUnlocked)
                nodeColor = Color.Gold;
            else if (canUnlock && isHovered)
                nodeColor = Color.LightGreen;
            else if (canUnlock)
                nodeColor = Color.Green;
            else
                nodeColor = Color.DarkGray;

            DrawCircle(spriteBatch, screenNodePos, NodeRadius, nodeColor);

            // Draw border if hovered
            if (isHovered)
            {
                DrawCircleOutline(spriteBatch, screenNodePos, NodeRadius + 3, Color.White, 2f);
            }
        }

        // Draw tooltip for hovered node
        if (_hoveredNode != null)
        {
            DrawTooltip(spriteBatch, _hoveredNode);
        }

        spriteBatch.End();
    }

    private void DrawTooltip(SpriteBatch spriteBatch, Node node)
    {
        var mouseState = Mouse.GetState();
        var tooltipPos = new Vector2(mouseState.X + 20, mouseState.Y);

        var lines = new[]
        {
            PlaceholderName,
            PlaceholderDescription,
            $"Cost: {node.Cost} SP",
            _grid.IsUnlocked(node) ? "[Unlocked]" :
                _grid.CanUnlock(node) ? "[Click to unlock]" : "[Cannot unlock]"
        };

        var maxWidth = lines.Max(line => _font.MeasureString(line).X);
        var lineHeight = _font.MeasureString("A").Y;
        var padding = 8;
        var tooltipWidth = maxWidth + padding * 2;
        var tooltipHeight = lineHeight * lines.Length + padding * 2;

        // Draw background
        var tooltipRect = new Rectangle((int)tooltipPos.X, (int)tooltipPos.Y,
            (int)tooltipWidth, (int)tooltipHeight);
        spriteBatch.Draw(PixelTexture, tooltipRect, Color.Black * 0.9f);

        // Draw border
        DrawRectangleOutline(spriteBatch, tooltipRect, Color.White, 1);

        // Draw text
        for (var i = 0; i < lines.Length; i++)
        {
            var textPos = tooltipPos + new Vector2(padding, padding + i * lineHeight);
            var color = i == lines.Length - 1 ? Color.Gray : Color.White;
            spriteBatch.DrawString(_font, lines[i], textPos, color);
        }
    }

    private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
    {
        var rect = new Rectangle((int)(center.X - radius), (int)(center.Y - radius),
            (int)(radius * 2), (int)(radius * 2));
        spriteBatch.Draw(PixelTexture, rect, color);
    }

    private void DrawCircleOutline(SpriteBatch spriteBatch, Vector2 center, float radius,
        Color color, float thickness)
    {
        const int segments = 16;
        for (var i = 0; i < segments; i++)
        {
            var angle1 = MathHelper.TwoPi * i / segments;
            var angle2 = MathHelper.TwoPi * (i + 1) / segments;

            var point1 = center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * radius;
            var point2 = center + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * radius;

            DrawLine(spriteBatch, point1, point2, color, thickness);
        }
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        var distance = Vector2.Distance(start, end);
        var angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

        spriteBatch.Draw(PixelTexture, start, null, color, angle, Vector2.Zero,
            new Vector2(distance, thickness), SpriteEffects.None, 0);
    }

    private void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        // Top
        spriteBatch.Draw(PixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(PixelTexture, new Rectangle(rect.X, rect.Y + rect.Height - thickness,
            rect.Width, thickness), color);
        // Left
        spriteBatch.Draw(PixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        spriteBatch.Draw(PixelTexture, new Rectangle(rect.X + rect.Width - thickness, rect.Y,
            thickness, rect.Height), color);
    }
}
