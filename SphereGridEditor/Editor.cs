using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ContentLibrary;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
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
    private int _nodeLevelInput = 1;
    private EdgeDirection? _pendingConnectionDirection;
    private string? _pendingNodeType;
    private Texture2D _pixel = null!;
    private KeyboardState _previousKeyboardState;
    private MouseState _previousMouseState;
    private PrimitiveRenderer _primitiveRenderer = null!;
    private Node? _selectedNode;
    private bool _showNodeCreationMenu;
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

        if (kbState.IsKeyDown(Keys.LeftControl) && kbState.IsKeyDown(Keys.P))
            Exit();

        // Copy code to clipboard on C key press
        if (kbState.IsKeyDown(Keys.C) && _previousKeyboardState.IsKeyUp(Keys.C)) CopyCodeToClipboard();

        // Delete selected node on Delete key
        if (kbState.IsKeyDown(Keys.Delete) && _previousKeyboardState.IsKeyUp(Keys.Delete) && _selectedNode != null)
        {
            DeleteNode(_selectedNode);
            _selectedNode = null;
        }

        // Show node creation menu on N key
        if (kbState.IsKeyDown(Keys.N) && _previousKeyboardState.IsKeyUp(Keys.N))
        {
            _showNodeCreationMenu = true;
            _nodeLevelInput = 1;
            Console.WriteLine("Node creation menu opened");
        }

        // Handle node creation menu interactions
        if (_showNodeCreationMenu)
        {
            // Handle number keys for immediate single-digit input
            for (var i = 1; i <= 9; i++)
            {
                var key = Keys.D0 + i;
                if (kbState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key)) _nodeLevelInput = i;
            }

            // Handle escape to close menu
            if (kbState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                _showNodeCreationMenu = false;
                _pendingNodeType = null;
                Console.WriteLine("Node creation cancelled");
            }
        }

        // Handle pending node placement
        if (_pendingNodeType != null)
        {
            // Place node on left click
            if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                var worldPosition = new Vector2(mouseState.X, mouseState.Y) - _cameraOffset;
                CreateNodeWithPowerup(_pendingNodeType, worldPosition, _nodeLevelInput);
                _pendingNodeType = null;
                Console.WriteLine("Node placed");
            }

            // Cancel on right click
            if (mouseState.RightButton == ButtonState.Pressed &&
                _previousMouseState.RightButton == ButtonState.Released)
            {
                _pendingNodeType = null;
                Console.WriteLine("Node placement cancelled");
            }

            // Cancel on escape
            if (kbState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                _pendingNodeType = null;
                Console.WriteLine("Node placement cancelled");
            }
        }

        // Delete edge on X key (after selecting direction with 1-6)
        if (kbState.IsKeyDown(Keys.X) &&
            _previousKeyboardState.IsKeyUp(Keys.X) &&
            _selectedNode != null &&
            _pendingConnectionDirection.HasValue &&
            _connectingFromNode == null)
        {
            DeleteEdge(_selectedNode, _pendingConnectionDirection.Value);
            _pendingConnectionDirection = null;
        }

        var mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);

        // Camera panning with right mouse button (only if not in connection mode)
        if (!_showNodeCreationMenu &&
            _connectingFromNode == null &&
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
            // Check if clicking on node creation menu buttons
            if (_showNodeCreationMenu)
            {
                var buttonClicked = GetClickedMenuButton(mouseScreenPos);

                if (buttonClicked != null && buttonClicked != "NodeLevelInput")
                {
                    // Set pending node type for placement
                    _pendingNodeType = buttonClicked;
                    _showNodeCreationMenu = false;
                    Console.WriteLine($"Selected {buttonClicked}. Left-click to place, right-click to cancel.");
                }
            }
            else if (_connectingFromNode != null && _hoveredNode != null && _pendingConnectionDirection.HasValue)
            {
                // Complete connection
                if (_hoveredNode != _connectingFromNode)
                {
                    var direction = _pendingConnectionDirection.Value;
                    var reverseDirection = direction.Opposite();

                    // Check if target node's reverse direction is already occupied
                    if (_hoveredNode.GetNeighbour(reverseDirection) != null)
                    {
                        Console.WriteLine(
                            $"Cannot connect: Target node already has a connection in direction {reverseDirection}");
                    }
                    else
                    {
                        _connectingFromNode.SetNeighbour(direction, _hoveredNode);
                        _hoveredNode.SetNeighbour(reverseDirection, _connectingFromNode);

                        Console.WriteLine(
                            $"Connected {_connectingFromNode.PowerUp?.GetType().Name ?? "Node"} to {_hoveredNode.PowerUp?.GetType().Name ?? "Node"} via {direction} (reverse: {reverseDirection})");
                    }
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
        // Or delete edge with X key after pressing direction
        if (!_showNodeCreationMenu &&
            _selectedNode != null)
        {
            if (kbState.IsKeyDown(Keys.D1) && _previousKeyboardState.IsKeyUp(Keys.D1))
                StartConnectionOrDelete(EdgeDirection.TopLeft);
            else if (kbState.IsKeyDown(Keys.D2) && _previousKeyboardState.IsKeyUp(Keys.D2))
                StartConnectionOrDelete(EdgeDirection.TopRight);
            else if (kbState.IsKeyDown(Keys.D3) && _previousKeyboardState.IsKeyUp(Keys.D3))
                StartConnectionOrDelete(EdgeDirection.MiddleLeft);
            else if (kbState.IsKeyDown(Keys.D4) && _previousKeyboardState.IsKeyUp(Keys.D4))
                StartConnectionOrDelete(EdgeDirection.MiddleRight);
            else if (kbState.IsKeyDown(Keys.D5) && _previousKeyboardState.IsKeyUp(Keys.D5))
                StartConnectionOrDelete(EdgeDirection.BottomLeft);
            else if (kbState.IsKeyDown(Keys.D6) && _previousKeyboardState.IsKeyUp(Keys.D6))
                StartConnectionOrDelete(EdgeDirection.BottomRight);
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
            var baseColor = node.BaseColor();
            var color = baseColor;

            if (isSelected) color = Color.White;
            else if (isHovered) color = Color.Lerp(baseColor, Color.White, 0.5f);

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
        var font = Content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Small);

        var helpLines = new[]
        {
            "N: New node | Del: Delete node | Middle-drag: Pan",
            "Select node, 1-6 (TopL/TopR/MidL/MidR/BotL/BotR), click target | X: Delete edge | C: Copy"
        };

        var y = 720f;

        for (var i = helpLines.Length - 1; i >= 0; i--)
        {
            var textSize = font.MeasureString(helpLines[i]);
            y -= textSize.Y + 2;
            _spriteBatch.DrawString(font, helpLines[i],
                new Vector2(10, y), Color.Gray, layerDepth: 0.01f);
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

        // Draw node creation menu with buttons
        if (_showNodeCreationMenu && font != null)
        {
            var menuPos = new Vector2(640 - 200, 200);
            var buttonWidth = 180;
            var buttonHeight = 30;
            var padding = 10;
            var lineHeight = font.MeasureString("A").Y;

            var buttons = new[]
            {
                ("DamageUp", "Damage Up"),
                ("SpeedUp", "Speed Up"),
                ("MaxHealthUp", "Max Health Up"),
                ("AttackSpeedUp", "Attack Speed Up"),
                ("PickupRadiusUp", "Pickup Radius Up"),
                ("RangeUp", "Range Up"),
                ("ShotCountUp", "Shot Count Up")
            };

            var menuWidth = buttonWidth + padding * 2 + 200;
            var menuHeight = (buttonHeight + 5) * (buttons.Length + 1) + padding * 3 + 40;

            // Draw background
            var menuRect = new Rectangle((int)menuPos.X, (int)menuPos.Y, menuWidth, menuHeight);
            _primitiveRenderer.DrawRectangle(_spriteBatch, menuRect, Color.Black * 0.95f);

            // Draw border
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(menuRect.X, menuRect.Y, menuRect.Width, 2), Color.Yellow);
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(menuRect.X, menuRect.Y + menuRect.Height - 2, menuRect.Width, 2), Color.Yellow);
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(menuRect.X, menuRect.Y, 2, menuRect.Height), Color.Yellow);
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(menuRect.X + menuRect.Width - 2, menuRect.Y, 2, menuRect.Height), Color.Yellow);

            // Title
            _spriteBatch.DrawString(font, "Create Node",
                menuPos + new Vector2(padding, padding), Color.Yellow, layerDepth: 0.01f);

            // Node Level display
            var inputY = menuPos.Y + padding * 2 + lineHeight;
            _spriteBatch.DrawString(font, "Node Level:",
                new Vector2(menuPos.X + padding, inputY), Color.White, layerDepth: 0.01f);

            var labelWidth = font.MeasureString("Node Level:").X;
            var boxSize = (int)lineHeight + 8;
            var inputRect = new Rectangle((int)menuPos.X + padding + (int)labelWidth + 10, (int)inputY - 4, boxSize,
                boxSize);
            _primitiveRenderer.DrawRectangle(_spriteBatch, inputRect, Color.DarkGray * 0.5f);
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(inputRect.X, inputRect.Y, inputRect.Width, 1), Color.Gray);
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(inputRect.X, inputRect.Y + inputRect.Height - 1, inputRect.Width, 1), Color.Gray);
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(inputRect.X, inputRect.Y, 1, inputRect.Height), Color.Gray);
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(inputRect.X + inputRect.Width - 1, inputRect.Y, 1, inputRect.Height), Color.Gray);

            var displayText = _nodeLevelInput.ToString();
            var textSize = font.MeasureString(displayText);
            _spriteBatch.DrawString(font, displayText,
                new Vector2(inputRect.X + (boxSize - textSize.X) / 2, inputRect.Y + (boxSize - textSize.Y) / 2),
                Color.White, layerDepth: 0.01f);

            // Button grid
            var buttonStartY = inputY + lineHeight + padding * 2;
            var mouseState = Mouse.GetState();
            var mousePos = new Vector2(mouseState.X, mouseState.Y);

            for (var i = 0; i < buttons.Length; i++)
            {
                var (id, label) = buttons[i];
                var buttonY = buttonStartY + i * (buttonHeight + 5);
                var buttonRect = new Rectangle((int)menuPos.X + padding, (int)buttonY, buttonWidth, buttonHeight);

                var isHovered = buttonRect.Contains(mousePos);
                var buttonColor = isHovered ? Color.Yellow * 0.3f : Color.DarkGray * 0.5f;

                _primitiveRenderer.DrawRectangle(_spriteBatch, buttonRect, buttonColor);

                // Button border
                var borderColor = isHovered ? Color.Yellow : Color.Gray;
                _primitiveRenderer.DrawRectangle(_spriteBatch,
                    new Rectangle(buttonRect.X, buttonRect.Y, buttonRect.Width, 1), borderColor);
                _primitiveRenderer.DrawRectangle(_spriteBatch,
                    new Rectangle(buttonRect.X, buttonRect.Y + buttonRect.Height - 1, buttonRect.Width, 1),
                    borderColor);
                _primitiveRenderer.DrawRectangle(_spriteBatch,
                    new Rectangle(buttonRect.X, buttonRect.Y, 1, buttonRect.Height), borderColor);
                _primitiveRenderer.DrawRectangle(_spriteBatch,
                    new Rectangle(buttonRect.X + buttonRect.Width - 1, buttonRect.Y, 1, buttonRect.Height),
                    borderColor);

                var labelSize = font.MeasureString(label);
                var textPos = new Vector2(buttonRect.X + (buttonRect.Width - labelSize.X) / 2,
                    buttonRect.Y + (buttonRect.Height - labelSize.Y) / 2);
                _spriteBatch.DrawString(font, label, textPos, Color.White, layerDepth: 0.01f);
            }

            // Help text
            var helpText = "Press 1-9 to set level, then click a node type";
            var helpY = buttonStartY + buttons.Length * (buttonHeight + 5) + padding;
            _spriteBatch.DrawString(font, helpText,
                new Vector2(menuPos.X + padding, helpY), Color.Gray, layerDepth: 0.01f);

            var escText = "ESC - Cancel";
            _spriteBatch.DrawString(font, escText,
                new Vector2(menuPos.X + padding, helpY + lineHeight + 5), Color.Gray, layerDepth: 0.01f);
        }

        // Draw placement mode indicator
        if (_pendingNodeType != null && font != null)
        {
            var msg =
                $"Placing {_pendingNodeType} (level {_nodeLevelInput}) - Left-click to place, Right-click to cancel";
            var msgSize = font.MeasureString(msg);
            var msgPos = new Vector2(640 - msgSize.X / 2, 10);

            // Draw background
            var bgRect = new Rectangle((int)msgPos.X - 10, (int)msgPos.Y - 5,
                (int)msgSize.X + 20, (int)msgSize.Y + 10);
            _primitiveRenderer.DrawRectangle(_spriteBatch, bgRect, Color.Black * 0.9f);

            _spriteBatch.DrawString(font, msg, msgPos, Color.Cyan, layerDepth: 0.01f);
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
        var factoryFunctions = """
                               Node DamageUp(int nodeLevel) => new(new DamageUp(nodeLevel * 0.25f), nodeLevel, nodeLevel);
                               Node SpeedUp(int nodeLevel) => new(new SpeedUp(nodeLevel * 0.2f), nodeLevel, nodeLevel);
                               Node MaxHealthUp(int nodeLevel) => new(new MaxHealthUp(nodeLevel * 2), nodeLevel, nodeLevel);
                               Node AttackSpeedUp(int nodeLevel) => new(new AttackSpeedUp(nodeLevel * 0.2f), nodeLevel, nodeLevel);
                               Node PickupRadiusUp(int nodeLevel) => new(new PickupRadiusUp(nodeLevel * 0.3f), nodeLevel, nodeLevel);
                               Node RangeUp(int nodeLevel) => new(new RangeUp(nodeLevel * 0.5f), nodeLevel, nodeLevel);
                               Node ShotCountUp(int nodeLevel) => new(new ShotCountUp(nodeLevel), nodeLevel, ShotCountCost(nodeLevel));

                               int ShotCountCost(int nodeLevel) => nodeLevel switch
                               {
                                   2 => 5,
                                   1 => 3,
                                   _ => throw new ArgumentOutOfRangeException(nameof(nodeLevel))
                               };

                               """;

        string FactoryFor(IPowerUp powerUp) => powerUp switch
        {
            DamageUp => "DamageUp",
            SpeedUp => "SpeedUp",
            MaxHealthUp => "MaxHealthUp",
            AttackSpeedUp => "AttackSpeedUp",
            PickupRadiusUp => "PickupRadiusUp",
            RangeUp => "RangeUp",
            ShotCountUp => "ShotCountUp",
            _ => throw new ArgumentOutOfRangeException(nameof(powerUp))
        };


        var sb = new StringBuilder(factoryFunctions);

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
        sb.AppendLine("var root = new Node(null, 0, 0);");
        foreach (var node in _grid.Nodes.Where(node => node != _grid.Root))
            if (node.PowerUp is { } powerUp)
                sb.AppendLine($"var {nodeNames[node]} = {FactoryFor(powerUp)}({node.Level});");

        sb.AppendLine();

        // Generate connections - output all directions explicitly
        foreach (var node in _grid.Nodes)
        foreach (var (direction, neighbor) in node.Neighbours)
            sb.AppendLine($"{nodeNames[node]}.SetNeighbour(EdgeDirection.{direction}, {nodeNames[neighbor]});");

        var code = sb.ToString();
        ClipboardHelper.Copy(code);
        Console.WriteLine("Code copied to clipboard!");
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

    private void CreateNodeWithPowerup(string powerupType, Vector2 worldPosition, int level)
    {
        Node DamageUp(int nodeLevel) => new(new DamageUp(nodeLevel * 0.25f), nodeLevel, nodeLevel);
        Node SpeedUp(int nodeLevel) => new(new SpeedUp(nodeLevel * 0.2f), nodeLevel, nodeLevel);
        Node MaxHealthUp(int nodeLevel) => new(new MaxHealthUp(nodeLevel * 2), nodeLevel, nodeLevel);
        Node AttackSpeedUp(int nodeLevel) => new(new AttackSpeedUp(nodeLevel * 0.2f), nodeLevel, nodeLevel);
        Node PickupRadiusUp(int nodeLevel) => new(new PickupRadiusUp(nodeLevel * 0.3f), nodeLevel, nodeLevel);
        Node RangeUp(int nodeLevel) => new(new RangeUp(nodeLevel * 0.5f), nodeLevel, nodeLevel);
        Node ShotCountUp(int nodeLevel) => new(new ShotCountUp(nodeLevel), nodeLevel, ShotCountCost(nodeLevel));

        int ShotCountCost(int nodeLevel) => nodeLevel switch
        {
            2 => 5,
            1 => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(nodeLevel))
        };

        var newNode = powerupType switch
        {
            "DamageUp" => DamageUp(level),
            "SpeedUp" => SpeedUp(level),
            "MaxHealthUp" => MaxHealthUp(level),
            "AttackSpeedUp" => AttackSpeedUp(level),
            "PickupRadiusUp" => PickupRadiusUp(level),
            "RangeUp" => RangeUp(level),
            "ShotCountUp" => ShotCountUp(level),
            _ => throw new ArgumentOutOfRangeException(nameof(powerupType))
        };

        _nodePositions[newNode] = worldPosition;

        // Add to grid using reflection
        var nodesField = typeof(SphereGrid).GetField("_nodes", BindingFlags.NonPublic | BindingFlags.Instance);
        var nodes = nodesField?.GetValue(_grid) as ISet<Node>;
        nodes?.Add(newNode);

        _selectedNode = newNode;
        Console.WriteLine($"Created {powerupType} (level {level}) node. Press 1-6 to connect to other nodes.");
    }

    private string? GetClickedMenuButton(Vector2 mousePos)
    {
        var font = _tooltipRenderer.GetType().GetField("_font",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(_tooltipRenderer) as SpriteFont;
        if (font == null) return null;

        var menuPos = new Vector2(640 - 200, 200);
        var buttonWidth = 180;
        var buttonHeight = 30;
        var padding = 10;
        var lineHeight = font.MeasureString("A").Y;

        var buttons = new[]
        {
            ("DamageUp", "Damage Up"),
            ("SpeedUp", "Speed Up"),
            ("MaxHealthUp", "Max Health Up"),
            ("AttackSpeedUp", "Attack Speed Up"),
            ("PickupRadiusUp", "Pickup Radius Up"),
            ("RangeUp", "Range Up"),
            ("ShotCountUp", "Shot Count Up")
        };

        // Check input field
        var inputY = menuPos.Y + padding * 2 + lineHeight;
        var inputRect = new Rectangle((int)menuPos.X + 100, (int)inputY - 2, 80, (int)lineHeight + 4);
        if (inputRect.Contains(mousePos))
            return "NodeLevelInput";

        // Check buttons
        var buttonStartY = inputY + lineHeight + padding * 2;

        for (var i = 0; i < buttons.Length; i++)
        {
            var (id, _) = buttons[i];
            var buttonY = buttonStartY + i * (buttonHeight + 5);
            var buttonRect = new Rectangle((int)menuPos.X + padding, (int)buttonY, buttonWidth, buttonHeight);

            if (buttonRect.Contains(mousePos))
                return id;
        }

        return null;
    }

    private void StartConnectionOrDelete(EdgeDirection direction)
    {
        if (_selectedNode == null) return;

        // Check if this direction already has a connection
        var existingNeighbour = _selectedNode.GetNeighbour(direction);

        if (existingNeighbour != null)
        {
            Console.WriteLine(
                $"Node already has a connection in direction {direction} to {existingNeighbour.PowerUp?.GetType().Name ?? "Node"}. Press X to delete it.");

            // Store the direction for deletion
            _pendingConnectionDirection = direction;
            return;
        }

        _connectingFromNode = _selectedNode;
        _pendingConnectionDirection = direction;
        Console.WriteLine(
            $"Starting connection from node with direction {direction}. Click on target node (or right-click to cancel).");
    }

    private static void DeleteEdge(Node fromNode, EdgeDirection direction)
    {
        var targetNode = fromNode.GetNeighbour(direction);

        if (targetNode == null)
        {
            Console.WriteLine("No edge to delete in that direction.");
            return;
        }

        // Remove the forward edge
        var neighboursField = typeof(Node).GetField("_neighbours", BindingFlags.NonPublic | BindingFlags.Instance);
        var neighbours = neighboursField?.GetValue(fromNode) as IDictionary<EdgeDirection, Node>;
        neighbours?.Remove(direction);

        // Remove the reverse edge
        var reverseDirection = direction.Opposite();
        var reverseNeighbours = neighboursField?.GetValue(targetNode) as IDictionary<EdgeDirection, Node>;
        reverseNeighbours?.Remove(reverseDirection);

        Console.WriteLine(
            $"Deleted edge from {fromNode.PowerUp?.GetType().Name ?? "Node"} to {targetNode.PowerUp?.GetType().Name ?? "Node"} (direction: {direction})");
    }
}