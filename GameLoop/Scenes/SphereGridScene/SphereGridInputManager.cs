using System;
using GameLoop.Input;
using GameLoop.Scenes.SphereGridScene.UI;
using Gameplay;
using Gameplay.Levelling.SphereGrid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridInputManager(
    IGlobalCommands globalCommands,
    GameFocusState focusState,
    SphereGridUi ui,
    SceneManager sceneManager)
    : BaseInputManager(globalCommands, focusState, sceneManager)
{
    private readonly TimeSpan _thumbstickNavigationCooldown = TimeSpan.FromMilliseconds(150);
    private TimeSpan _currentThumbstickNavigationCooldown = TimeSpan.Zero;
    private bool _isPanning;

    internal override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (ShouldSkipInput()) return;

        if (WasPressedThisFrame(Keys.Escape) ||
            WasPressedThisFrame(Keys.Space) ||
            WasPressedThisFrame(Keys.Tab) ||
            WasPressedThisFrame(Buttons.Back) ||
            WasPressedThisFrame(Buttons.B)) GlobalCommands.CloseSphereGrid();

        _isPanning = MouseState.MiddleButton == ButtonState.Pressed;
        if (_isPanning)
        {
            var mouseDelta = new Vector2(
                MouseState.X - PreviousMouseState.X,
                MouseState.Y - PreviousMouseState.Y
            );
            ui.Camera.Position -= mouseDelta;
        }

        if (MouseState.LeftButton == ButtonState.Pressed &&
            PreviousMouseState.LeftButton == ButtonState.Released)
            ui.UnlockHoveredNode();

        if (WasPressedThisFrame(Buttons.A))
            ui.UnlockFocussedNode();

        // Handle panning with gamepad right thumbstick
        var thumbstickInput = GamePadState.ThumbSticks.Right;
        if (thumbstickInput.LengthSquared() > 0.02f)
            ui.Camera.Position -= new Vector2(-thumbstickInput.X, thumbstickInput.Y) * 32f;

        ui.HideFocus = CurrentInputMethod is InputMethod.KeyboardMouse;

        ui.UpdateHoveredNode(MouseState.Position.ToVector2());
        UpdateFocussedNode(gameTime);
    }

    private void UpdateFocussedNode(GameTime gameTime)
    {
        var inputDirection = GamePadState.ThumbSticks.Left;
        inputDirection.Y *= -1f; // Flip Y to match screen coordinates

        if (inputDirection.LengthSquared() < 0.02f) return;

        _currentThumbstickNavigationCooldown -= gameTime.ElapsedGameTime;
        if (_currentThumbstickNavigationCooldown > TimeSpan.Zero) return;

        var currentPosition = ui.NodePositions[ui.FocusedNode];

        Node? best = null;
        var bestScore = float.MinValue;

        var inputDirNormalized = Vector2.Normalize(inputDirection);

        const float minAngleCos = 0.5f; // ~60° cone (more forgiving)
        const float distanceBias = 1.5f; // strongly favour closer nodes

        foreach (var (node, position) in ui.NodePositions)
        {
            if (node == ui.FocusedNode)
                continue;

            if (!ui.IsVisible(position))
                continue;

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
                best = node;
            }
        }

        if (best == null) return;

        ui.FocusedNode = best;
        ui.HideFocus = false;
        _currentThumbstickNavigationCooldown = _thumbstickNavigationCooldown;
    }
}