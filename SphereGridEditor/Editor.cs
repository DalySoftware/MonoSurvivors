using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering;
using Gameplay.Rendering.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using ToolTip = Gameplay.Rendering.Tooltips.ToolTip;

namespace SphereGridEditor;

public class Editor : Game
{
    private readonly Dictionary<Node, Vector2> _nodePositions = new();
    private Vector2 _cameraOffset = new(400, 300);
    private Node? _connectingFromNode;

    private SphereGrid _grid = null!;
    private Node? _hoveredNode;
    private EdgeDirection? _pendingConnectionDirection;
    private Texture2D _pixel = null!;
    private KeyboardState _previousKeyboardState;
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

        // Copy code to clipboard on C key press
        if (kbState.IsKeyDown(Keys.C) && _previousKeyboardState.IsKeyUp(Keys.C)) CopyCodeToClipboard();

        // Delete selected node on Delete key
        if (kbState.IsKeyDown(Keys.Delete) && _previousKeyboardState.IsKeyUp(Keys.Delete) && _selectedNode != null)
        {
            DeleteNode(_selectedNode);
            _selectedNode = null;
        }

        // Create new node on N key
        if (kbState.IsKeyDown(Keys.N) && _previousKeyboardState.IsKeyUp(Keys.N))
            CreateNewNode(new Vector2(mouseState.X, mouseState.Y) - _cameraOffset);

        var mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);

        // Camera panning with right mouse button (only if not in connection mode)
        if (_connectingFromNode == null &&
            mouseState.MiddleButton == ButtonState.Pressed &&
            _previousMouseState.MiddleButton == ButtonState.Pressed)
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

        // Selection with left click
        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            if (_connectingFromNode != null && _hoveredNode != null && _pendingConnectionDirection.HasValue)
            {
                // Complete connection
                if (_hoveredNode != _connectingFromNode)
                {
                    var direction = _pendingConnectionDirection.Value;
                    _connectingFromNode.SetNeighbour(direction, _hoveredNode);

                    // Also set the reverse connection (bidirectional edge)
                    var reverseDirection = direction.Opposite();
                    _hoveredNode.SetNeighbour(reverseDirection, _connectingFromNode);

                    Console.WriteLine(
                        $"Connected {_connectingFromNode.PowerUp?.GetType().Name ?? "Node"} to {_hoveredNode.PowerUp?.GetType().Name ?? "Node"} via {direction} (reverse: {reverseDirection})");
                }

                _connectingFromNode = null;
                _pendingConnectionDirection = null;
            }
            else
            {
                _selectedNode = _hoveredNode;
            }
        }

        // Cancel connection mode on right click
        if (mouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released)
            if (_connectingFromNode != null)
            {
                Console.WriteLine("Connection cancelled");
                _connectingFromNode = null;
                _pendingConnectionDirection = null;
            }

        // Start connection on key press 1-6 while node is selected
        if (_selectedNode != null)
        {
            if (kbState.IsKeyDown(Keys.D1) && _previousKeyboardState.IsKeyUp(Keys.D1))
                StartConnection(EdgeDirection.TopLeft);
            else if (kbState.IsKeyDown(Keys.D2) && _previousKeyboardState.IsKeyUp(Keys.D2))
                StartConnection(EdgeDirection.TopRight);
            else if (kbState.IsKeyDown(Keys.D3) && _previousKeyboardState.IsKeyUp(Keys.D3))
                StartConnection(EdgeDirection.MiddleLeft);
            else if (kbState.IsKeyDown(Keys.D4) && _previousKeyboardState.IsKeyUp(Keys.D4))
                StartConnection(EdgeDirection.MiddleRight);
            else if (kbState.IsKeyDown(Keys.D5) && _previousKeyboardState.IsKeyUp(Keys.D5))
                StartConnection(EdgeDirection.BottomLeft);
            else if (kbState.IsKeyDown(Keys.D6) && _previousKeyboardState.IsKeyUp(Keys.D6))
                StartConnection(EdgeDirection.BottomRight);
        }

        _previousMouseState = mouseState;
        _previousKeyboardState = kbState;
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

        // Draw pending connection line
        if (_connectingFromNode != null && _nodePositions.ContainsKey(_connectingFromNode))
        {
            var startPos = _nodePositions[_connectingFromNode] + _cameraOffset;
            var mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            DrawLine(startPos, mousePos, Color.Yellow * 0.7f, 3);
        }

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
            var connectionDetails = _hoveredNode.Neighbours
                .Select(kvp => $"{kvp.Key}")
                .ToList();

            var tooltipLines = new List<ToolTipBodyLine>
            {
                new($"Cost: {_hoveredNode.Cost}"),
                new($"PowerUp: {_hoveredNode.PowerUp?.GetType().Name ?? "None"}"),
                new($"Connections: {_hoveredNode.Neighbours.Count}")
            };

            foreach (var conn in connectionDetails)
                tooltipLines.Add(new ToolTipBodyLine($"  - {conn}", Color.DarkGray));

            var tooltip = new ToolTip("Node Info", tooltipLines);
            _tooltipRenderer.DrawTooltip(_spriteBatch, tooltip);
        }

        // Draw selected node info in top-left corner
        if (_selectedNode != null)
            DrawInfoPanel(new Vector2(10, 10), "SELECTED NODE", [
                $"Cost: {_selectedNode.Cost}",
                $"PowerUp: {_selectedNode.PowerUp?.GetType().Name ?? "None"}",
                $"Connections: {_selectedNode.Neighbours.Count}"
            ], Color.Yellow);

        // Draw help text at bottom
        var font = _tooltipRenderer.GetType().GetField("_font",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(_tooltipRenderer) as SpriteFont;

        if (font != null)
        {
            var helpLines = new[]
            {
                "N: New node | Del: Delete | Middle-click-drag: Pan",
                "Select node, press 1-6 (TopL/TopR/MidL/MidR/BotL/BotR), click target | C: Copy code"
            };

            var y = 720f;

            for (var i = helpLines.Length - 1; i >= 0; i--)
            {
                var textSize = font.MeasureString(helpLines[i]);
                y -= textSize.Y + 2;
                _spriteBatch.DrawString(font, helpLines[i],
                    new Vector2(10, y), Color.Gray, layerDepth: 0.01f);
            }
        }

        // Draw connection mode indicator
        if (_connectingFromNode != null && font != null)
        {
            var msg = $"Connecting with {_pendingConnectionDirection} - Click target node";
            var msgSize = font.MeasureString(msg);
            var msgPos = new Vector2(640 - msgSize.X / 2, 10);

            // Draw background
            var bgRect = new Rectangle((int)msgPos.X - 10, (int)msgPos.Y - 5,
                (int)msgSize.X + 20, (int)msgSize.Y + 10);
            _primitiveRenderer.DrawRectangle(_spriteBatch, bgRect, Color.Black * 0.9f);

            _spriteBatch.DrawString(font, msg, msgPos, Color.Yellow, layerDepth: 0.01f);
        }

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

    private void CopyCodeToClipboard()
    {
        var sb = new StringBuilder();

        // Add the hardcoded helper functions
        sb.AppendLine("Node DamageUp(int nodeLevel) => new(new DamageUp(nodeLevel * 0.25f), nodeLevel);");
        sb.AppendLine("Node SpeedUp(int nodeLevel) => new(new SpeedUp(nodeLevel * 0.2f), nodeLevel);");
        sb.AppendLine("Node MaxHealthUp(int nodeLevel) => new(new MaxHealthUp(nodeLevel * 2), nodeLevel);");
        sb.AppendLine("Node AttackSpeedUp(int nodeLevel) => new(new AttackSpeedUp(nodeLevel * 0.2f), nodeLevel);");
        sb.AppendLine("Node PickupRadiusUp(int nodeLevel) => new(new PickupRadiusUp(nodeLevel * 0.3f), nodeLevel);");
        sb.AppendLine("Node RangeUp(int nodeLevel) => new(new RangeUp(nodeLevel * 0.5f), nodeLevel);");
        sb.AppendLine();
        sb.AppendLine("Node ShotCountUp(int nodeLevel) => nodeLevel switch");
        sb.AppendLine("{");
        sb.AppendLine("    2 => new Node(new ShotCountUp(2), 5),");
        sb.AppendLine("    1 => new Node(new ShotCountUp(1), 3),");
        sb.AppendLine("    _ => throw new ArgumentOutOfRangeException(nameof(nodeLevel))");
        sb.AppendLine("};");
        sb.AppendLine();

        // Generate node graph code
        var nodeNames = new Dictionary<Node, string>();
        var nodeIndex = 0;

        // Assign names to all nodes
        foreach (var node in _grid.Nodes)
            if (node == _grid.Root)
            {
                nodeNames[node] = "root";
            }
            else
            {
                var powerUpName = node.PowerUp?.GetType().Name ?? "Node";
                nodeNames[node] = $"{powerUpName.ToLower()}{nodeIndex++}";
            }

        // Generate node creation code
        foreach (var node in _grid.Nodes)
            if (node == _grid.Root)
            {
                sb.AppendLine("var root = new Node(null, 0);");
            }
            else
            {
                var powerUpType = node.PowerUp?.GetType().Name;
                if (powerUpType != null) sb.AppendLine($"var {nodeNames[node]} = {powerUpType}(1);");
            }

        sb.AppendLine();

        // Generate connections
        var processedEdges = new HashSet<(Node, Node)>();

        foreach (var node in _grid.Nodes)
        foreach (var (direction, neighbor) in node.Neighbours)
            // Only output each edge once
            if (!processedEdges.Contains((neighbor, node)))
            {
                sb.AppendLine($"{nodeNames[node]}.SetNeighbour(EdgeDirection.{direction}, {nodeNames[neighbor]});");
                processedEdges.Add((node, neighbor));
            }

        var code = sb.ToString();
        ClipboardHelper.Copy(code);
        Console.WriteLine("Code copied to clipboard!");
        Console.WriteLine(code);
    }

    private void DeleteNode(Node node)
    {
        if (node == _grid.Root)
        {
            Console.WriteLine("Cannot delete root node!");
            return;
        }

        // Remove from position tracking
        _nodePositions.Remove(node);

        // Remove all connections to this node from other nodes
        foreach (var otherNode in _grid.Nodes)
        {
            var connectionsToRemove = otherNode.Neighbours
                .Where(kvp => kvp.Value == node)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var direction in connectionsToRemove)
            {
                // Use reflection to remove from the private dictionary
                var neighboursField =
                    typeof(Node).GetField("_neighbours", BindingFlags.NonPublic | BindingFlags.Instance);
                var neighbours = neighboursField?.GetValue(otherNode) as IDictionary<EdgeDirection, Node>;
                neighbours?.Remove(direction);
            }
        }

        // Remove from grid nodes collection using reflection
        var nodesField = typeof(SphereGrid).GetField("_nodes", BindingFlags.NonPublic | BindingFlags.Instance);
        var nodes = nodesField?.GetValue(_grid) as ISet<Node>;
        nodes?.Remove(node);

        Console.WriteLine($"Deleted node with powerup: {node.PowerUp?.GetType().Name ?? "None"}");
    }

    private void CreateNewNode(Vector2 worldPosition)
    {
        // Create a new empty node
        var newNode = new Node(null, 1);
        _nodePositions[newNode] = worldPosition;

        // Add to grid using reflection
        var nodesField = typeof(SphereGrid).GetField("_nodes", BindingFlags.NonPublic | BindingFlags.Instance);
        var nodes = nodesField?.GetValue(_grid) as ISet<Node>;
        nodes?.Add(newNode);

        _selectedNode = newNode;
        Console.WriteLine("Created new node. Press 1-6 to connect to other nodes.");
    }

    private void StartConnection(EdgeDirection direction)
    {
        if (_selectedNode == null) return;

        // Check if this direction already has a connection
        if (_selectedNode.GetNeighbour(direction) != null)
        {
            Console.WriteLine(
                $"Node already has a connection in direction {direction}. Delete it first or choose another direction.");
            return;
        }

        _connectingFromNode = _selectedNode;
        _pendingConnectionDirection = direction;
        Console.WriteLine(
            $"Starting connection from node with direction {direction}. Click on target node (or right-click to cancel).");
    }
}