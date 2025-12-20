using System;
using GameLoop.Input;
using GameLoop.Scenes.SphereGridScene.UI;
using Gameplay;
using Gameplay.Levelling.SphereGrid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridInputManager(IGlobalCommands globalCommands, SphereGridUi ui)
    : BaseInputManager
{
    private readonly TimeSpan _thumbstickNavigationCooldown = TimeSpan.FromMilliseconds(150);
    private TimeSpan _currentThumbstickNavigationCooldown = TimeSpan.Zero;
    private bool _isPanning;

    internal override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (WasPressedThisFrame(Keys.Escape) ||
            WasPressedThisFrame(Keys.Tab) ||
            WasPressedThisFrame(Buttons.Back) ||
            WasPressedThisFrame(Buttons.B)) globalCommands.CloseSphereGrid();

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
            ui.Camera.Position -= new Vector2(-thumbstickInput.X, thumbstickInput.Y) * 8f;

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
        const float maxAngleCos = 0.766f; // 40 degrees each way

        foreach (var (_, neighbour) in ui.FocusedNode.Neighbours)
        {
            var neighbourPosition = ui.NodePositions[neighbour];
            if (!ui.IsVisible(neighbourPosition)) continue;

            var dir = Vector2.Normalize(neighbourPosition - currentPosition);
            var score = Vector2.Dot(dir, inputDirNormalized);

            if (score > bestScore && score >= maxAngleCos)
            {
                bestScore = score;
                best = neighbour;
            }
        }

        if (best == null) return;

        ui.FocusedNode = best;
        ui.HideFocus = false;

        // // Keep focus roughly centered
        // _cameraOffset = -focusPos;

        _currentThumbstickNavigationCooldown = _thumbstickNavigationCooldown;
    }
}