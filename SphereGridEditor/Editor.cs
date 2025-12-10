using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering;
using Gameplay.Rendering.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SphereGridEditor;

public class Editor : Game
{
    private readonly Dictionary<Node, Vector2> _nodePositions = new();
    private Vector2 _cameraOffset = new(400, 300);

    private SphereGrid _grid = null!;
    private Node? _hoveredNode;
    private Texture2D _pixel = null!;
    private MouseState _previousMouseState;
    private PrimitiveRenderer _primitiveRenderer = null!;
    private Node? _selectedNode;
    private SpriteBatch _spriteBatch = null!;
    private ToolTipRenderer _tooltipRenderer = null!;

    public Editor()
    {
        var graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        Content.RootDirectory = "ContentLibrary";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Console.WriteLine("Initialize called");

        try
        {
            _grid = SphereGrid.Create(_ => { });
            Console.WriteLine($"Grid created with {_grid.Nodes.Count} nodes");
            LayoutNodes();
            Console.WriteLine("Layout complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Initialize: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            throw;
        }

        base.Initialize();
        Console.WriteLine("Initialize complete");
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white pixel for drawing shapes
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

        _primitiveRenderer = new PrimitiveRenderer(GraphicsDevice);
        _tooltipRenderer = new ToolTipRenderer(_primitiveRenderer, Content);
    }

    private void LayoutNodes()
    {
        var root = _grid.Root;
        _nodePositions[root] = Vector2.Zero;

        // Simple radial layout
        var directionVectors = new Dictionary<EdgeDirection, Vector2>
        {
            [EdgeDirection.MiddleRight] = new(150, 0),
            [EdgeDirection.TopRight] = new(120, -100),
            [EdgeDirection.BottomRight] = new(120, 100),
            [EdgeDirection.MiddleLeft] = new(-150, 0),
            [EdgeDirection.TopLeft] = new(-120, -100),
            [EdgeDirection.BottomLeft] = new(-120, 100)
        };

        var visited = new HashSet<Node>();
        var queue = new Queue<(Node node, Vector2 position)>();
        queue.Enqueue((root, Vector2.Zero));

        while (queue.Count > 0)
        {
            var (node, pos) = queue.Dequeue();
            if (!visited.Add(node)) continue;

            _nodePositions[node] = pos;

            foreach (var (dir, neighbor) in node.Neighbours)
                if (!visited.Contains(neighbor))
                {
                    var offset = directionVectors.GetValueOrDefault(dir, Vector2.Zero);
                    queue.Enqueue((neighbor, pos + offset));
                }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        var kbState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        if (kbState.IsKeyDown(Keys.Escape))
            Exit();

        var mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);

        // Camera panning with right mouse button
        if (mouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Pressed)
        {
            var delta = mouseState.Position - _previousMouseState.Position;
            _cameraOffset += delta.ToVector2();
        }

        // Find hovered node (compare in screen space)
        _hoveredNode = null;

        foreach (var (node, pos) in _nodePositions)
        {
            var screenPos = pos + _cameraOffset;

            if (Vector2.Distance(mouseScreenPos, screenPos) < 20)
            {
                _hoveredNode = node;
                break;
            }
        }

        // Selection
        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            _selectedNode = _hoveredNode;

        _previousMouseState = mouseState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(20, 20, 30));

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw edges
        foreach (var (node, pos) in _nodePositions)
        foreach (var (_, neighbor) in node.Neighbours)
            if (_nodePositions.TryGetValue(neighbor, out var neighborPos))
                // Only draw each edge once
                if (_nodePositions[node].GetHashCode() < neighborPos.GetHashCode())
                    DrawLine(pos + _cameraOffset, neighborPos + _cameraOffset, Color.Gray * 0.5f, 2);

        // Draw nodes
        foreach (var (node, pos) in _nodePositions)
        {
            var screenPos = pos + _cameraOffset;
            var isRoot = node == _grid.Root;
            var isSelected = node == _selectedNode;
            var isHovered = node == _hoveredNode;

            var radius = isRoot ? 25f : 20f;
            var color = isRoot ? Color.Gold :
                node.PowerUp != null ? Color.LightBlue : Color.Gray;

            if (isSelected) color = Color.White;
            else if (isHovered) color = Color.Lerp(color, Color.White, 0.5f);

            DrawCircle(screenPos, radius, color);
            DrawCircle(screenPos, radius - 2, new Color(20, 20, 30));
            DrawCircle(screenPos, radius - 4, color * 0.3f);
        }

        // Draw tooltip for hovered node (follows mouse)
        if (_hoveredNode != null)
        {
            var tooltip = new ToolTip(
                "Node Info",
                [
                    new ToolTipBodyLine($"Cost: {_hoveredNode.Cost}"),
                    new ToolTipBodyLine($"PowerUp: {_hoveredNode.PowerUp?.GetType().Name ?? "None"}"),
                    new ToolTipBodyLine($"Connections: {_hoveredNode.Neighbours.Count}")
                ]
            );

            _tooltipRenderer.DrawTooltip(_spriteBatch, tooltip);
        }

        // Draw selected node info in top-left corner
        if (_selectedNode != null)
            DrawInfoPanel(new Vector2(10, 10), "SELECTED NODE", [
                $"Cost: {_selectedNode.Cost}",
                $"PowerUp: {_selectedNode.PowerUp?.GetType().Name ?? "None"}",
                $"Connections: {_selectedNode.Neighbours.Count}"
            ], Color.Yellow);

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawCircle(Vector2 center, float radius, Color color)
    {
        var segments = 32;

        for (var i = 0; i < segments; i++)
        {
            var angle1 = (float)i / segments * MathF.PI * 2;
            var angle2 = (float)(i + 1) / segments * MathF.PI * 2;

            var p1 = center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * radius;
            var p2 = center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * radius;

            DrawLine(p1, p2, color, 2);
        }
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1)
    {
        var distance = Vector2.Distance(start, end);
        var angle = MathF.Atan2(end.Y - start.Y, end.X - start.X);

        _spriteBatch.Draw(_pixel, start, null, color, angle, Vector2.Zero,
            new Vector2(distance, thickness), SpriteEffects.None, 0);
    }

    private void DrawInfoPanel(Vector2 position, string title, string[] lines, Color textColor)
    {
        var font = _tooltipRenderer.GetType().GetField("_font",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(_tooltipRenderer) as SpriteFont;

        if (font == null) return;

        var lineHeight = font.MeasureString("A").Y;
        const int padding = 8;

        // Calculate panel size
        var titleWidth = font.MeasureString(title).X;
        var maxWidth = lines.Select(line => font.MeasureString(line).X).Append(titleWidth).Max();

        var panelWidth = maxWidth + padding * 2;
        var panelHeight = lineHeight * (lines.Length + 1) + padding * 2;

        // Draw background
        var rect = new Rectangle((int)position.X, (int)position.Y, (int)panelWidth, (int)panelHeight);
        _primitiveRenderer.DrawRectangle(_spriteBatch, rect, Color.Black * 0.9f);

        // Draw title
        var textPos = position + new Vector2(padding, padding);
        _spriteBatch.DrawString(font, title, textPos, Color.White, layerDepth: 0.01f);

        // Draw lines
        for (var i = 0; i < lines.Length; i++)
        {
            textPos = position + new Vector2(padding, padding + (i + 1) * lineHeight);
            _spriteBatch.DrawString(font, lines[i], textPos, textColor, layerDepth: 0.01f);
        }
    }
}