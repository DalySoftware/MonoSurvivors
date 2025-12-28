using System;
using System.Linq;
using GameLoop.Input;
using GameLoop.Scenes.SphereGridScene.UI;
using Gameplay;
using Gameplay.Levelling.SphereGrid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    SphereGridUi ui,
    SceneManager sceneManager)
    : BaseInputManager(globalCommands, inputState, sceneManager)
{
    private readonly TimeSpan _thumbstickNavigationCooldown = TimeSpan.FromMilliseconds(150);
    private TimeSpan _currentThumbstickNavigationCooldown = TimeSpan.Zero;
    private bool _isPanning;

    internal void Update(GameTime gameTime)
    {
        if (ShouldSkipInput()) return;

        if (WasPressedThisFrame(Keys.Escape) ||
            WasPressedThisFrame(Keys.Space) ||
            WasPressedThisFrame(Keys.Tab) ||
            WasPressedThisFrame(Buttons.Back) ||
            WasPressedThisFrame(Buttons.B)) GlobalCommands.CloseSphereGrid();

        _isPanning = InputState.MouseState.MiddleButton == ButtonState.Pressed;
        if (_isPanning)
        {
            var mouseDelta = new Vector2(
                InputState.MouseState.X - InputState.PreviousMouseState.X,
                InputState.MouseState.Y - InputState.PreviousMouseState.Y
            );
            ui.Camera.Position -= mouseDelta;
        }

        if (InputState.MouseState.LeftButton == ButtonState.Pressed &&
            InputState.PreviousMouseState.LeftButton == ButtonState.Released)
            ui.UnlockHoveredNode();

        if (WasPressedThisFrame(Buttons.A))
            ui.UnlockFocussedNode();

        if (WasPressedThisFrame(Buttons.Y) || WasPressedThisFrame(Keys.T))
            ui.Camera.Position = Vector2.Zero;

        // Handle panning with gamepad right thumbstick
        var thumbstickInput = InputState.GamePadState.ThumbSticks.Right;
        if (thumbstickInput.LengthSquared() > 0.02f)
            ui.Camera.Position -= new Vector2(-thumbstickInput.X, thumbstickInput.Y) * 32f;

        ui.HideFocus = CurrentInputMethod is InputMethod.KeyboardMouse;

        UpdateHoveredNode();
        UpdateFocussedNode(gameTime);
    }

    private void UpdateHoveredNode() =>
        ui.UpdateHoveredNode(CurrentInputMethod == InputMethod.KeyboardMouse
            ? InputState.MouseState.Position.ToVector2()
            : null);

    private void UpdateFocussedNode(GameTime gameTime)
    {
        var inputDirection = GetInputDirection(gameTime);
        if (inputDirection == Vector2.Zero)
            return;

        var currentPosition = ui.NodePositions[ui.FocusedNode];

        Node? bestCandidate = null;
        var bestScore = float.MinValue;

        var inputDirNormalized = Vector2.Normalize(inputDirection);

        const float minAngleCos = 0.5f; // ~60° cone (more forgiving)
        const float distanceBias = 1.5f; // strongly favour closer nodes

        var candidates = ui.NodePositions
            .Where(kvp => kvp.Key != ui.FocusedNode && ui.IsVisible(kvp.Value));

        foreach (var (candidate, position) in candidates)
        {
            var toNode = position - currentPosition;
            var distance = toNode.Length();

            if (distance <= 0.0001f)
                continue;

            var dir = toNode / distance;
            var alignment = Vector2.Dot(dir, inputDirNormalized);

            // Reject nodes too far off-axis
            if (alignment < minAngleCos)
                continue;

            // Alignment matters, but distance dominates
            var distanceScore = 1f / MathF.Pow(distance, distanceBias);
            var score = alignment * distanceScore;

            if (score > bestScore)
            {
                bestScore = score;
                bestCandidate = candidate;
            }
        }

        if (bestCandidate == null) return;

        ui.FocusedNode = bestCandidate;
        ui.HideFocus = false;
        _currentThumbstickNavigationCooldown = _thumbstickNavigationCooldown;
    }

    private Vector2 GetInputDirection(GameTime gameTime)
    {
        // Thumbstick
        var stick = InputState.GamePadState.ThumbSticks.Left;
        stick.Y *= -1f;

        if (stick.LengthSquared() >= 0.02f)
        {
            _currentThumbstickNavigationCooldown -= gameTime.ElapsedGameTime;
            if (_currentThumbstickNavigationCooldown > TimeSpan.Zero) return Vector2.Zero;

            _currentThumbstickNavigationCooldown = _thumbstickNavigationCooldown;
            return stick;
        }

        // Reset cooldown when stick released
        _currentThumbstickNavigationCooldown = TimeSpan.Zero;

        // D-pad (edge-triggered)
        var direction = Vector2.Zero;

        if (WasPressedThisFrame(Buttons.DPadUp)) direction += Vector2.UnitY * -1f;
        if (WasPressedThisFrame(Buttons.DPadDown)) direction += Vector2.UnitY;
        if (WasPressedThisFrame(Buttons.DPadLeft)) direction += Vector2.UnitX * -1f;
        if (WasPressedThisFrame(Buttons.DPadRight)) direction += Vector2.UnitX;

        return direction;
    }
}