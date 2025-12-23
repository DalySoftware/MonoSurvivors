using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentLibrary;
using GameLoop.Scenes.SphereGridScene.UI;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Levelling.SphereGrid.Generation;
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
    private readonly Dictionary<PowerUpCategory, Rectangle> _categoryButtons = [];
    private readonly Dictionary<NodeRarity, Rectangle> _rarityButtons = [];

    private readonly Dictionary<Node, Vector2> _nodePositions = new();
    private readonly Dictionary<Node, (PowerUpCategory? category, NodeRarity rarity)> _nodeMetadata = new();
    private readonly HashSet<Node> _nodes = [];
    private Rectangle _createButton;
    private Vector2 _cameraOffset;
    private Node? _connectingFromNode;
    private SpriteFont _font = null!;

    private Node? _hoveredNode;
    private PowerUpCategory _selectedCategory = PowerUpCategory.Damage;
    private NodeRarity _selectedRarity = NodeRarity.Common;
    private EdgeDirection? _pendingConnectionDirection;
    private (PowerUpCategory category, NodeRarity rarity)? _pendingNodePlacement;
    private Texture2D _pixel = null!;
    private KeyboardState _previousKeyboardState;
    private MouseState _previousMouseState;
    private PrimitiveRenderer _primitiveRenderer = null!;
    private Node? _selectedNode;
    private bool _showNodeCreationMenu;
    private SpriteBatch _spriteBatch = null!;
    private ToolTipRenderer _tooltipRenderer = null!;
    private Node _root = null!;

    public Editor()
    {
        var graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = 1920;
        graphics.PreferredBackBufferHeight = 1080;
        Content.RootDirectory = "ContentLibrary";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Console.WriteLine("Initialize called");

        // Initialize camera offset to center of screen
        _cameraOffset = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);

        // Load template from GridFactory
        LoadTemplateIntoEditor(TemplateFactory.CreateTemplate());

        base.Initialize();
        Console.WriteLine("Initialize complete");
    }

    private void LoadTemplateIntoEditor(GridTemplate template)
    {
        // 1. Create nodes from template
        var nodeMap = new Dictionary<int, Node>();
        foreach (var nt in template.Nodes)
        {
            var node = new Node(null, nt.Rarity);
            nodeMap[nt.Id] = node;
            _nodeMetadata[node] = (nt.Category, nt.Rarity);
        }

        // 2. Wire edges
        foreach (var nt in template.Nodes)
        {
            var from = nodeMap[nt.Id];
            foreach (var (direction, targetId) in nt.Neighbours)
            {
                var to = nodeMap[targetId];
                if (from.GetNeighbour(direction) == null)
                {
                    from.SetNeighbour(direction, to);
                    to.SetNeighbour(direction.Opposite(), from);
                }
            }
        }

        // 3. Create grid with root
        _root = nodeMap[template.RootId];
        _nodes.Add(_root);

        // 4. Add all nodes to grid using reflection
        foreach (var node in nodeMap.Values)
            _nodes.Add(node);

        // 5. Calculate initial positions
        LayoutNodes();

        Console.WriteLine($"Loaded template with {template.Nodes.Count} nodes");
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white pixel for drawing shapes
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

        _font = Content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Small);

        _primitiveRenderer = new PrimitiveRenderer(Content, GraphicsDevice);
        _tooltipRenderer = new ToolTipRenderer(_primitiveRenderer, Content);
    }

    private void LayoutNodes()
    {
        _nodePositions[_root] = Vector2.Zero;

        var positioner = new SphereGridPositioner(_root);
        foreach (var (node, position) in positioner.NodePositions().Positions)
            _nodePositions[node] = position;
    }

    protected override void Update(GameTime gameTime)
    {
        var kbState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        if (kbState.IsKeyDown(Keys.LeftControl) && kbState.IsKeyDown(Keys.P))
            Exit();

        if (kbState.IsKeyDown(Keys.C) && _previousKeyboardState.IsKeyUp(Keys.C)) CopyTemplateToClipboard();

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
            _selectedCategory = PowerUpCategory.Damage;
            _selectedRarity = NodeRarity.Common;
            Console.WriteLine("Node creation menu opened");
        }

        // Handle node creation menu interactions
        if (_showNodeCreationMenu)
            // Handle escape to close menu
            if (kbState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                _showNodeCreationMenu = false;
                _pendingNodePlacement = null;
                Console.WriteLine("Node creation cancelled");
            }

        // Handle pending node placement
        if (_pendingNodePlacement != null)
        {
            // Place node on left click
            if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                var worldPosition = new Vector2(mouseState.X, mouseState.Y) - _cameraOffset;
                CreateNodeTemplate(_pendingNodePlacement.Value.category, _pendingNodePlacement.Value.rarity,
                    worldPosition);
                _pendingNodePlacement = null;
                Console.WriteLine("Node template placed");
            }

            // Cancel on right click
            if (mouseState.RightButton == ButtonState.Pressed &&
                _previousMouseState.RightButton == ButtonState.Released)
            {
                _pendingNodePlacement = null;
                Console.WriteLine("Node placement cancelled");
            }

            // Cancel on escape
            if (kbState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                _pendingNodePlacement = null;
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
            var nodeRadius = node == _root ? 50f : 40f;

            if (Vector2.Distance(mouseScreenPos, screenPos) < nodeRadius)
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
                var (categoryClicked, rarityClicked) = GetClickedMenuButton(mouseScreenPos);

                if (categoryClicked.HasValue)
                {
                    _selectedCategory = categoryClicked.Value;
                }
                else if (rarityClicked.HasValue)
                {
                    _selectedRarity = rarityClicked.Value;
                }
                else if (GetClickedCreateButton(mouseScreenPos))
                {
                    // Set pending node placement
                    _pendingNodePlacement = (_selectedCategory, _selectedRarity);
                    _showNodeCreationMenu = false;
                    Console.WriteLine(
                        $"Selected {_selectedCategory} ({_selectedRarity}). Left-click to place, right-click to cancel.");
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

                        Console.WriteLine($"Connected nodes via {direction} (reverse: {reverseDirection})");
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

        if (!_showNodeCreationMenu && kbState.IsKeyDown(Keys.T) && _previousKeyboardState.IsKeyUp(Keys.T))
            LayoutNodes();

        _previousMouseState = mouseState;
        _previousKeyboardState = kbState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(20, 20, 30));

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

        // Draw edges
        foreach (var (node, pos) in _nodePositions)
        foreach (var (direction, neighbor) in node.Neighbours)
            if (_nodePositions.TryGetValue(neighbor, out var neighborPos))
                // Only draw each edge once
                if (_nodePositions[node].GetHashCode() < neighborPos.GetHashCode())
                {
                    var fromOffset = GetDirectionOffset(direction, node == _root ? 50f : 40f);
                    var toOffset = GetDirectionOffset(direction.Opposite(), neighbor == _root ? 50f : 40f);
                    _primitiveRenderer.DrawLine(_spriteBatch, pos + fromOffset + _cameraOffset,
                        neighborPos + toOffset + _cameraOffset,
                        Color.Gray * 0.5f, 2);
                }

        // Draw pending connection line
        if (_connectingFromNode != null &&
            _nodePositions.ContainsKey(_connectingFromNode) &&
            _pendingConnectionDirection.HasValue)
        {
            var radius = _connectingFromNode == _root ? 50f : 40f;
            var offset = GetDirectionOffset(_pendingConnectionDirection.Value, radius);
            var startPos = _nodePositions[_connectingFromNode] + offset + _cameraOffset;
            var mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            _primitiveRenderer.DrawLine(_spriteBatch, startPos, mousePos, Color.Yellow * 0.7f, 3,
                Layers.HelpText - 0.05f);
        }

        // Draw nodes
        foreach (var (node, pos) in _nodePositions)
        {
            var screenPos = pos + _cameraOffset;
            var isRoot = node == _root;
            var isSelected = node == _selectedNode;
            var isHovered = node == _hoveredNode;

            // Determine radius based on rarity
            float baseRadius;
            if (isRoot)
                baseRadius = 50f;
            else
                baseRadius = node.Rarity switch
                {
                    NodeRarity.Legendary => 45f,
                    NodeRarity.Rare => 40f,
                    _ => 35f,
                };

            // Color based on category
            var baseColor = isRoot ? Color.Gold : CategoryColor(node);
            var color = baseColor;

            if (isSelected) color = Color.White;
            else if (isHovered) color = Color.Lerp(baseColor, Color.White, 0.5f);

            // Draw main circle
            DrawCircle(screenPos, baseRadius, color, Layers.Node);
            DrawCircle(screenPos, baseRadius - 2, new Color(20, 20, 30), Layers.Node + 0.001f);
            DrawCircle(screenPos, baseRadius - 4, color * 0.3f, Layers.Node + 0.002f);

            // Draw extra border for rare/legendary nodes
            if (!isRoot)
            {
                if (node.Rarity == NodeRarity.Rare)
                {
                    DrawCircle(screenPos, baseRadius + 3, color * 0.6f, Layers.Node - 0.001f);
                }
                else if (node.Rarity == NodeRarity.Legendary)
                {
                    DrawCircle(screenPos, baseRadius + 3, color * 0.7f, Layers.Node - 0.001f);
                    DrawCircle(screenPos, baseRadius + 6, color * 0.4f, Layers.Node - 0.002f);
                }
            }

            // Draw category abbreviation and rarity
            var abbrev = isRoot ? "Root" : CategoryAbbreviation(node);
            var rarityText = isRoot ? "" : RarityAbbreviation(node.Rarity);

            if (!string.IsNullOrEmpty(abbrev))
            {
                var abbrevSize = _font.MeasureString(abbrev);
                var abbrevPos = screenPos - new Vector2(abbrevSize.X / 2, abbrevSize.Y + 2);
                _spriteBatch.DrawString(_font, abbrev, abbrevPos, Color.White, layerDepth: Layers.NodeText);
            }

            if (!string.IsNullOrEmpty(rarityText))
            {
                var raritySize = _font.MeasureString(rarityText);
                var rarityPos = screenPos - new Vector2(raritySize.X / 2, -2);
                _spriteBatch.DrawString(_font, rarityText, rarityPos, Color.LightGray, layerDepth: Layers.NodeText);
            }
        }

        // Draw tooltip for hovered node (follows mouse)
        if (_hoveredNode != null)
        {
            var connectionDetails = _hoveredNode.Neighbours
                .Select(kvp => $"{kvp.Key}")
                .ToList();

            var category = _nodeMetadata.TryGetValue(_hoveredNode, out var meta) ? meta.category.ToString() : "Unknown";
            var tooltipLines = new List<ToolTipBodyLine>
            {
                new($"Category: {category}"),
                new($"Rarity: {_hoveredNode.Rarity}"),
                new($"Connections: {_hoveredNode.Neighbours.Count}"),
            };

            foreach (var conn in connectionDetails)
                tooltipLines.Add(new ToolTipBodyLine($"  - {conn}", Color.DarkGray));

            var tooltip = new ToolTip("Template Node", tooltipLines);
            _tooltipRenderer.DrawTooltip(_spriteBatch, tooltip);
        }

        // Draw selected node info in top-left corner
        if (_selectedNode != null)
        {
            var category = _nodeMetadata.TryGetValue(_selectedNode, out var meta)
                ? meta.category.ToString()
                : "Unknown";
            DrawInfoPanel(new Vector2(10, 10), "SELECTED NODE", [
                $"Category: {category}",
                $"Cost: {_selectedNode.Cost} SP",
                $"Rarity: {_selectedNode.Rarity}",
                $"Connections: {_selectedNode.Neighbours.Count}",
            ], Color.Yellow);
        }

        // Draw help text at bottom
        var font = Content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Small);

        var helpLines = new[]
        {
            "N: New node | Del: Delete node | Middle-drag: Pan | C: Copy template",
            "Select node, 1-6 (TopL/TopR/MidL/MidR/BotL/BotR), click target | X: Delete edge | T: Recalculate layout",
        };

        var y = (float)GraphicsDevice.Viewport.Height;

        for (var i = helpLines.Length - 1; i >= 0; i--)
        {
            var textSize = font.MeasureString(helpLines[i]);
            y -= textSize.Y + 2;
            _spriteBatch.DrawString(font, helpLines[i],
                new Vector2(10, y), Color.Gray, layerDepth: Layers.HelpText);
        }

        // Draw connection mode indicator
        if (_connectingFromNode != null && font != null)
        {
            var msg = $"Connecting with {_pendingConnectionDirection} - Click target node";
            var msgSize = font.MeasureString(msg);
            var msgPos = new Vector2(GraphicsDevice.Viewport.Width / 2f - msgSize.X / 2, 10);

            // Draw background
            var bgRect = new Rectangle((int)msgPos.X - 10, (int)msgPos.Y - 5,
                (int)msgSize.X + 20, (int)msgSize.Y + 10);
            _primitiveRenderer.DrawRectangle(_spriteBatch, bgRect, Color.Black * 0.9f, Layers.HelpText - 0.01f);

            _spriteBatch.DrawString(font, msg, msgPos, Color.Yellow, layerDepth: Layers.HelpText);
        }

        // Draw node creation menu with category and rarity selection
        if (_showNodeCreationMenu && font != null)
        {
            var menuPos = new Vector2(GraphicsDevice.Viewport.Width / 2f - 250,
                GraphicsDevice.Viewport.Height / 2f - 200);
            var buttonWidth = 180;
            var buttonHeight = 30;
            var padding = 10;
            var lineHeight = font.MeasureString("A").Y;

            var categories = Enum.GetValues<PowerUpCategory>();
            var rarities = Enum.GetValues<NodeRarity>();

            var menuWidth = buttonWidth * 2 + padding * 4;
            var menuHeight = (buttonHeight + 5) * Math.Max(categories.Length, rarities.Length) +
                             padding * 4 +
                             (int)lineHeight * 3 +
                             60;

            // Draw background
            var menuRect = new Rectangle((int)menuPos.X, (int)menuPos.Y, menuWidth, menuHeight);
            _primitiveRenderer.DrawRectangle(_spriteBatch, menuRect, Color.Black * 0.95f, Layers.CreateNode - 0.001f);

            // Draw border
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(menuRect.X, menuRect.Y, menuRect.Width, 2), Color.Yellow, Layers.CreateNode);
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(menuRect.X, menuRect.Y + menuRect.Height - 2, menuRect.Width, 2), Color.Yellow,
                Layers.CreateNode);
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(menuRect.X, menuRect.Y, 2, menuRect.Height), Color.Yellow, Layers.CreateNode);
            _primitiveRenderer.DrawRectangle(_spriteBatch,
                new Rectangle(menuRect.X + menuRect.Width - 2, menuRect.Y, 2, menuRect.Height), Color.Yellow,
                Layers.CreateNode);

            // Title
            _spriteBatch.DrawString(font, "Create Node Template",
                menuPos + new Vector2(padding, padding), Color.Yellow, layerDepth: Layers.CreateNodeText);

            var currentY = menuPos.Y + padding * 2 + lineHeight;
            var mouseState = Mouse.GetState();
            var mousePos = new Vector2(mouseState.X, mouseState.Y);

            // Category column
            _spriteBatch.DrawString(font, "Category:",
                new Vector2(menuPos.X + padding, currentY), Color.White, layerDepth: Layers.CreateNodeText);

            currentY += lineHeight + 5;
            _categoryButtons.Clear();
            for (var i = 0; i < categories.Length; i++)
            {
                var category = categories[i];
                var buttonRect = new Rectangle((int)menuPos.X + padding, (int)currentY, buttonWidth, buttonHeight);
                _categoryButtons[category] = buttonRect;

                var isSelected = category == _selectedCategory;
                var isHovered = buttonRect.Contains(mousePos);
                var buttonColor = isSelected ? Color.Green * 0.5f :
                    isHovered ? Color.Yellow * 0.3f : Color.DarkGray * 0.5f;

                _primitiveRenderer.DrawRectangle(_spriteBatch, buttonRect, buttonColor, Layers.CreateNodeButton);

                // Button border
                var borderColor = isSelected ? Color.Green : isHovered ? Color.Yellow : Color.Gray;
                DrawButtonBorder(_spriteBatch, buttonRect, borderColor);

                var labelSize = font.MeasureString(category.ToString());
                var textPos = new Vector2(buttonRect.X + (buttonRect.Width - labelSize.X) / 2,
                    buttonRect.Y + (buttonRect.Height - labelSize.Y) / 2);
                _spriteBatch.DrawString(font, category.ToString(), textPos, Color.White,
                    layerDepth: Layers.CreateNodeText);

                currentY += buttonHeight + 5;
            }

            // Rarity column
            currentY = menuPos.Y + padding * 2 + lineHeight;
            _spriteBatch.DrawString(font, "Rarity:",
                new Vector2(menuPos.X + padding * 3 + buttonWidth, currentY), Color.White,
                layerDepth: Layers.CreateNodeText);

            currentY += lineHeight + 5;
            _rarityButtons.Clear();
            for (var i = 0; i < rarities.Length; i++)
            {
                var rarity = rarities[i];
                var buttonRect = new Rectangle((int)menuPos.X + padding * 3 + buttonWidth, (int)currentY, buttonWidth,
                    buttonHeight);
                _rarityButtons[rarity] = buttonRect;

                var isSelected = rarity == _selectedRarity;
                var isHovered = buttonRect.Contains(mousePos);
                var buttonColor = isSelected ? Color.Green * 0.5f :
                    isHovered ? Color.Yellow * 0.3f : Color.DarkGray * 0.5f;

                _primitiveRenderer.DrawRectangle(_spriteBatch, buttonRect, buttonColor, Layers.CreateNodeButton);

                // Button border
                var borderColor = isSelected ? Color.Green : isHovered ? Color.Yellow : Color.Gray;
                DrawButtonBorder(_spriteBatch, buttonRect, borderColor);

                var labelSize = font.MeasureString(rarity.ToString());
                var textPos = new Vector2(buttonRect.X + (buttonRect.Width - labelSize.X) / 2,
                    buttonRect.Y + (buttonRect.Height - labelSize.Y) / 2);
                _spriteBatch.DrawString(font, rarity.ToString(), textPos, Color.White,
                    layerDepth: Layers.CreateNodeText);

                currentY += buttonHeight + 5;
            }

            // Create button at bottom
            currentY = menuPos.Y + menuHeight - padding - buttonHeight - 5;
            var createButtonRect = new Rectangle((int)menuPos.X + padding, (int)currentY, menuWidth - padding * 2,
                buttonHeight);
            _createButton = createButtonRect;

            var isCreateHovered = createButtonRect.Contains(mousePos);
            var createButtonColor = isCreateHovered ? Color.Cyan * 0.5f : Color.DarkBlue * 0.7f;
            _primitiveRenderer.DrawRectangle(_spriteBatch, createButtonRect, createButtonColor,
                Layers.CreateNodeButton);
            DrawButtonBorder(_spriteBatch, createButtonRect, isCreateHovered ? Color.Cyan : Color.Blue);

            var createText = $"Create {_selectedCategory} ({_selectedRarity})";
            var createTextSize = font.MeasureString(createText);
            var createTextPos = new Vector2(createButtonRect.X + (createButtonRect.Width - createTextSize.X) / 2,
                createButtonRect.Y + (createButtonRect.Height - createTextSize.Y) / 2);
            _spriteBatch.DrawString(font, createText, createTextPos, Color.White, layerDepth: Layers.CreateNodeText);

            // Help text
            var escText = "ESC - Cancel";
            _spriteBatch.DrawString(font, escText,
                new Vector2(menuPos.X + padding, currentY - lineHeight - 10), Color.Gray,
                layerDepth: Layers.CreateNodeText);
        }

        // Draw placement mode indicator
        if (_pendingNodePlacement != null && font != null)
        {
            var msg =
                $"Placing {_pendingNodePlacement.Value.category} ({_pendingNodePlacement.Value.rarity}) - Left-click to place, Right-click to cancel";
            var msgSize = font.MeasureString(msg);
            var msgPos = new Vector2(GraphicsDevice.Viewport.Width / 2f - msgSize.X / 2, 10);

            // Draw background
            var bgRect = new Rectangle((int)msgPos.X - 10, (int)msgPos.Y - 5,
                (int)msgSize.X + 20, (int)msgSize.Y + 10);
            _primitiveRenderer.DrawRectangle(_spriteBatch, bgRect, Color.Black * 0.9f, Layers.HelpText - 0.001f);

            _spriteBatch.DrawString(font, msg, msgPos, Color.Cyan, layerDepth: Layers.HelpText);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private static Vector2 GetDirectionOffset(EdgeDirection direction, float radius) => direction switch
    {
        EdgeDirection.TopLeft => new Vector2(-radius * 0.5f, -radius * 0.866f),
        EdgeDirection.TopRight => new Vector2(radius * 0.5f, -radius * 0.866f),
        EdgeDirection.MiddleLeft => new Vector2(-radius, 0),
        EdgeDirection.MiddleRight => new Vector2(radius, 0),
        EdgeDirection.BottomLeft => new Vector2(-radius * 0.5f, radius * 0.866f),
        EdgeDirection.BottomRight => new Vector2(radius * 0.5f, radius * 0.866f),
        _ => Vector2.Zero,
    };

    private void DrawCircle(Vector2 center, float radius, Color color, float layerDepth)
    {
        var segments = 32;

        for (var i = 0; i < segments; i++)
        {
            var angle1 = (float)i / segments * MathF.PI * 2;
            var angle2 = (float)(i + 1) / segments * MathF.PI * 2;

            var p1 = center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * radius;
            var p2 = center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * radius;

            _primitiveRenderer.DrawLine(_spriteBatch, p1, p2, color, 2, layerDepth);
        }
    }

    private void DrawInfoPanel(Vector2 position, string title, string[] lines, Color textColor)
    {
        var lineHeight = _font.MeasureString("A").Y;
        const int padding = 8;

        // Calculate panel size
        var titleWidth = _font.MeasureString(title).X;
        var maxWidth = lines.Select(line => _font.MeasureString(line).X).Append(titleWidth).Max();

        var panelWidth = maxWidth + padding * 2;
        var panelHeight = lineHeight * (lines.Length + 1) + padding * 2;

        // Draw background
        var rect = new Rectangle((int)position.X, (int)position.Y, (int)panelWidth, (int)panelHeight);
        _primitiveRenderer.DrawRectangle(_spriteBatch, rect, Color.Black * 0.9f, Layers.InfoPanel);

        // Draw title
        var textPos = position + new Vector2(padding, padding);
        _spriteBatch.DrawString(_font, title, textPos, Color.White, layerDepth: Layers.InfoPanel + 0.1f);

        // Draw lines
        for (var i = 0; i < lines.Length; i++)
        {
            textPos = position + new Vector2(padding, padding + (i + 1) * lineHeight);
            _spriteBatch.DrawString(_font, lines[i], textPos, textColor, layerDepth: Layers.InfoPanel + 0.01f);
        }
    }

    private void CopyTemplateToClipboard()
    {
        var template = BuildTemplate(); // convert _grid + _nodeMetadata → GridTemplate

        var sb = new StringBuilder();
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Gameplay.Levelling.PowerUps;");
        sb.AppendLine("// ReSharper disable RedundantEmptyObjectOrCollectionInitializer");
        sb.AppendLine("namespace Gameplay.Levelling.SphereGrid.Generation;");
        sb.AppendLine("public static class TemplateFactory {");

        sb.AppendLine("internal static GridTemplate CreateTemplate() => new()");
        sb.AppendLine("{");
        sb.AppendLine($"    RootId = {template.RootId},");
        sb.AppendLine("    Nodes =");
        sb.AppendLine("    [");

        foreach (var nodeTemplate in template.Nodes)
        {
            sb.AppendLine("        new NodeTemplate");
            sb.AppendLine("        {");
            sb.AppendLine($"            Id = {nodeTemplate.Id},");
            sb.AppendLine($"            Category = PowerUpCategory.{nodeTemplate.Category},");
            sb.AppendLine($"            Rarity = NodeRarity.{nodeTemplate.Rarity},");
            sb.AppendLine("            Neighbours = new Dictionary<EdgeDirection,int>");
            sb.AppendLine("            {");
            foreach (var kvp in nodeTemplate.Neighbours)
                sb.AppendLine($"                {{ EdgeDirection.{kvp.Key}, {kvp.Value} }},");
            sb.AppendLine("            },");
            sb.AppendLine("        },");
        }

        sb.AppendLine("    ],");
        sb.AppendLine("};");

        sb.AppendLine("}");

        ClipboardHelper.Copy(sb.ToString());
        Console.WriteLine("C# GridTemplate copied to clipboard!");
    }


    private void DeleteNode(Node node)
    {
        if (node == _root)
        {
            Console.WriteLine("Cannot delete root node!");
            return;
        }

        // Remove from position tracking
        _nodePositions.Remove(node);
        _nodeMetadata.Remove(node);

        foreach (var (direction, neighbour) in node.Neighbours)
            neighbour.SetNeighbour(direction.Opposite(), null);

        _nodes.Remove(node);

        Console.WriteLine($"Deleted template node (Cost: {node.Cost})");
    }


    private void CreateNodeTemplate(PowerUpCategory category, NodeRarity rarity, Vector2 worldPosition)
    {
        // Create a placeholder node (will be replaced by GridFactory randomization)
        var newNode = new Node(null, rarity);
        _nodePositions[newNode] = worldPosition;
        _nodeMetadata[newNode] = (category, rarity);

        _nodes.Add(newNode);

        _selectedNode = newNode;
        Console.WriteLine($"Created {category} ({rarity}) template node. Press 1-6 to connect to other nodes.");
    }

    private (PowerUpCategory? category, NodeRarity? rarity) GetClickedMenuButton(Vector2 mousePos)
    {
        foreach (var kvp in _categoryButtons.Where(kvp => kvp.Value.Contains(mousePos)))
            return (kvp.Key, null);

        foreach (var kvp in _rarityButtons.Where(kvp => kvp.Value.Contains(mousePos)))
            return (null, kvp.Key);

        return (null, null);
    }

    private GridTemplate BuildTemplate()
    {
        var nodes = _nodes.ToList();

        var idMap = new Dictionary<Node, int>();
        for (var i = 0; i < nodes.Count; i++)
            idMap[nodes[i]] = i;

        var template = new GridTemplate
        {
            RootId = idMap[_root],
        };

        foreach (var node in nodes)
        {
            var meta = _nodeMetadata.TryGetValue(node, out var m) ? m : throw new Exception();

            var nodeTemplate = new NodeTemplate
            {
                Id = idMap[node],
                Category = meta.Item1!.Value,
                Rarity = meta.Item2,
            };

            foreach (var (dir, neighbour) in node.Neighbours)
                // Only emit one direction (avoid duplicates)
                if (idMap[node] < idMap[neighbour])
                    nodeTemplate.Neighbours[dir] = idMap[neighbour];

            template.Nodes.Add(nodeTemplate);
        }

        return template;
    }


    private bool GetClickedCreateButton(Vector2 mousePos) => _createButton.Contains(mousePos);

    private void DrawButtonBorder(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        _primitiveRenderer.DrawRectangle(spriteBatch,
            new Rectangle(rect.X, rect.Y, rect.Width, 1),
            color, Layers.CreateNodeButton + 0.001f);
        _primitiveRenderer.DrawRectangle(spriteBatch,
            new Rectangle(rect.X, rect.Y + rect.Height - 1, rect.Width, 1),
            color, Layers.CreateNodeButton + 0.001f);
        _primitiveRenderer.DrawRectangle(spriteBatch,
            new Rectangle(rect.X, rect.Y, 1, rect.Height),
            color, Layers.CreateNodeButton + 0.001f);
        _primitiveRenderer.DrawRectangle(spriteBatch,
            new Rectangle(rect.X + rect.Width - 1, rect.Y, 1, rect.Height),
            color, Layers.CreateNodeButton + 0.001f);
    }

    private void StartConnectionOrDelete(EdgeDirection direction)
    {
        if (_selectedNode == null) return;

        // Check if this direction already has a connection
        var existingNeighbour = _selectedNode.GetNeighbour(direction);

        if (existingNeighbour != null)
        {
            Console.WriteLine($"Node already has a connection in direction {direction}. Press X to delete it.");

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

        fromNode.SetNeighbour(direction, null);
        targetNode.SetNeighbour(direction.Opposite(), null);

        Console.WriteLine($"Deleted edge between nodes (direction: {direction})");
    }

    private Color CategoryColor(Node node)
    {
        if (!_nodeMetadata.TryGetValue(node, out var meta))
            return Color.Gray;

        return meta.category switch
        {
            PowerUpCategory.Damage => new Color(220, 50, 50), // Red
            PowerUpCategory.DamageEffects => new Color(255, 140, 0), // Orange
            PowerUpCategory.Health => new Color(50, 200, 50), // Green
            PowerUpCategory.Speed => new Color(100, 200, 255), // Light Blue
            PowerUpCategory.Utility => new Color(200, 200, 100), // Yellow
            PowerUpCategory.Crit => new Color(200, 50, 200), // Magenta
            PowerUpCategory.WeaponUnlock => new Color(150, 100, 255), // Purple
            _ => Color.Gray,
        };
    }

    private static string RarityAbbreviation(NodeRarity rarity) => rarity switch
    {
        NodeRarity.Common => "C",
        NodeRarity.Rare => "R",
        NodeRarity.Legendary => "L",
        _ => "",
    };

    private string CategoryAbbreviation(Node node)
    {
        if (!_nodeMetadata.TryGetValue(node, out var meta))
            return "?";

        return meta.category switch
        {
            PowerUpCategory.Damage => "DMG",
            PowerUpCategory.DamageEffects => "FX",
            PowerUpCategory.Health => "HP",
            PowerUpCategory.Speed => "SPD",
            PowerUpCategory.Utility => "UTL",
            PowerUpCategory.Crit => "CRIT",
            PowerUpCategory.WeaponUnlock => "WPN",
            _ => "?",
        };
    }

    private static class Layers
    {
        internal const float Node = 0.30f;
        internal const float NodeText = 0.35f;

        internal const float InfoPanel = 0.60f;
        internal const float HelpText = 0.60f;
        internal const float CreateNode = 0.80f;
        internal const float CreateNodeButton = 0.85f;
        internal const float CreateNodeText = 0.90f;
    }
}