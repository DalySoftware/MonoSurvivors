using System;
using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using GameLoop.Scenes.SphereGridScene;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.UI;

/// <summary>
///     UI overlay for the sphere grid levelling system
/// </summary>
internal class SphereGridUi
{
    private readonly SpriteFont _fontLarge;
    private readonly SpriteFont _fontSmall;
    private readonly GraphicsDevice _graphicsDevice;


    private readonly SphereGrid _grid;
    private readonly Texture2D _gridNodeLarge;
    private readonly Texture2D _gridNodeSmall;
    private readonly SphereGridInputManager _inputManager;
    private readonly IReadOnlyDictionary<Node, Vector2> _nodePositions;

    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly ToolTipRenderer _toolTipRenderer;

    private Node? _hoveredNode;
    private MouseState _previousMouseState;

    internal SphereGridUi(ContentManager content, GraphicsDevice graphicsDevice, SphereGrid grid,
        PrimitiveRenderer primitiveRenderer, SphereGridInputManager inputManager)
    {
        _grid = grid;
        _primitiveRenderer = primitiveRenderer;
        _toolTipRenderer = new ToolTipRenderer(_primitiveRenderer, content);
        _inputManager = inputManager;
        _graphicsDevice = graphicsDevice;
        _fontSmall = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Small);
        _fontLarge = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Large);
        _gridNodeSmall = content.Load<Texture2D>(Paths.Images.GridNode);
        _gridNodeLarge = content.Load<Texture2D>(Paths.Images.GridNodeLarge);

        ScreenSpaceOrigin = _graphicsDevice.Viewport.Bounds.Center.ToVector2();

        const float nodeSpacing = 160f;
        _nodePositions = new SphereGridPositioner(_grid, nodeSpacing).NodePositions();
    }

    private Vector2 ScreenSpaceOrigin => field + _inputManager.CameraOffset;


    internal void Update()
    {
        var mouseState = Mouse.GetState();

        _hoveredNode = _grid.Nodes.FirstOrDefault(node =>
        {
            if (!_nodePositions.TryGetValue(node, out var nodePos)) return false;

            var screenPos = ScreenSpaceOrigin + nodePos;
            var mousePos = new Vector2(mouseState.X, mouseState.Y);

            var radius = NodeTexture(node).Width / 2f;
            return Vector2.Distance(mousePos, screenPos) <= radius;
        });

        // Click to unlock
        if (!_inputManager.IsPanning &&
            _hoveredNode != null &&
            mouseState.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
            _grid.Unlock(_hoveredNode);

        _previousMouseState = mouseState;
    }

    internal void Draw(SpriteBatch spriteBatch)
    {
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

        foreach (var node in _grid.Nodes)
        {
            if (!_nodePositions.TryGetValue(node, out var nodePos)) continue;

            var screenNodePos = ScreenSpaceOrigin + nodePos;

            foreach (var (_, neighbor) in node.Neighbours)
            {
                if (!_nodePositions.TryGetValue(neighbor, out var neighborPos)) continue;

                var screenNeighborPos = ScreenSpaceOrigin + neighborPos;
                var isUnlocked = _grid.IsUnlocked(node) && _grid.IsUnlocked(neighbor);

                var color = isUnlocked ? Color.Gold : Color.Gray * 0.5f;
                _primitiveRenderer.DrawLine(spriteBatch, screenNodePos, screenNeighborPos, color, 8f, Layers.Edges);
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

        var baseColor = node.BaseColor();
        var color =
            isUnlocked ? baseColor.ShiftLightness(-0.25f) :
            canUnlock ? baseColor.ShiftChroma(-0f).ShiftLightness(0.3f) :
            baseColor.ShiftChroma(-0.12f);

        var screenNodePos = ScreenSpaceOrigin + nodePos;
        var texture = NodeTexture(node);
        DrawNode(spriteBatch, texture, screenNodePos, color);

        if (node == _hoveredNode)
        {
            // Draw a highlight on top
            DrawNode(spriteBatch, texture, screenNodePos, Color.White * 0.4f);
            DrawTooltip(spriteBatch, _hoveredNode);
        }
    }

    private Texture2D NodeTexture(Node node) => node.Cost switch
    {
        >= 3 => _gridNodeLarge,
        _ => _gridNodeSmall
    };

    private void DrawNode(SpriteBatch spriteBatch, Texture2D sprite, Vector2 center, Color color) =>
        spriteBatch.Draw(sprite, center, origin: sprite.Centre, color: color, layerDepth: Layers.Nodes);

    private void DrawTooltip(SpriteBatch spriteBatch, Node node)
    {
        if (node.PowerUp is not { } powerUp) return;

        var title = TitleFor(powerUp);

        ToolTipBodyLine[] body =
        [
            new(DescriptionFor(powerUp)),
            new($"Cost: {node.Cost} SP"),
            UnlockTextFor(node)
        ];

        var tooltip = new ToolTip(title, body);
        _toolTipRenderer.DrawTooltip(spriteBatch, tooltip, Layers.ToolTip);
    }

    private static string TitleFor(IPowerUp powerUp) => powerUp switch
    {
        MaxHealthUp => "Increase Max Health",
        SpeedUp => "Increase Speed",
        PickupRadiusUp => "Increase Pickup Radius",
        DamageUp => "Increase Damage",
        AttackSpeedUp => "Increase Attack Speed",
        ShotCountUp => "Increase Shot Count",
        RangeUp => "Increase Range",
        LifeStealUp => "Increase Life Steal",
        ExperienceUp => "Increase Experience Multiplier",
        CritChanceUp => "Increase Critical Hit Chance",
        CritDamageUp => "Increase Critical Hit Damage",
        PierceUp => "Pierce more enemies",
        ProjectileSpeedUp => "Increase Projectile Speed",
        _ => throw new ArgumentOutOfRangeException(nameof(powerUp))
    };

    private static string DescriptionFor(IPowerUp powerUp) => powerUp switch
    {
        MaxHealthUp maxHealthUp => $"Increase Max Health by {(maxHealthUp.Value / 2).HeartLabel()}",
        SpeedUp speedUp => $"Increase Speed by {speedUp.Value:P0}",
        PickupRadiusUp pickupRadiusUp => $"Increase Pickup Radius by {pickupRadiusUp.Value:P0}",
        DamageUp damageUp => $"Increase Damage by {damageUp.Value:P0}",
        AttackSpeedUp attackSpeedUp => $"Increase Attack Speed by {attackSpeedUp.Value:P0}",
        ShotCountUp shotCountUp => $"Fire {shotCountUp.ExtraShots} extra shots",
        RangeUp rangeUp => $"Increase Range by {rangeUp.Value:P0}",
        LifeStealUp => "Increase LifeSteal",
        ExperienceUp experienceUp => $"Increase Experience Multiplier by {experienceUp.Value:P0}",
        CritChanceUp critChanceUp => $"Increase Critical Hit Chance by {critChanceUp.Value:P0}",
        CritDamageUp critDamageUp => $"Increase Critical Hit Damage by {critDamageUp.Value:P0}",
        PierceUp pierceUp => $"Projectiles pierce {pierceUp.Value} more {pierceUp.Value.EnemiesLabel()}",
        ProjectileSpeedUp projectileSpeedUp => $"Increase Projectile Speed by {projectileSpeedUp.Value:P0}",
        _ => throw new ArgumentOutOfRangeException(nameof(powerUp))
    };

    private ToolTipBodyLine UnlockTextFor(Node node) =>
        _grid.IsUnlocked(node) ? new ToolTipBodyLine("[Unlocked]", Color.LawnGreen) :
        _grid.CanUnlock(node) ? new ToolTipBodyLine("[Click to unlock]", Color.Turquoise) :
        new ToolTipBodyLine("[Cannot unlock]", Color.DimGray);

    private static class Layers
    {
        internal const float Edges = 0.40f;
        internal const float Nodes = 0.50f;
        internal const float Title = 0.80f;
        internal const float ToolTip = 0.90f;
    }
}

internal static class Pluralization
{
    extension(int value)
    {
        internal string HeartLabel() => $"{value} {(value == 1 ? "heart" : "hearts")}";
        internal string EnemiesLabel() => value == 1 ? "enemy" : "enemies";
    }
}