using System;
using GameLoop.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridInputManager : BaseInputManager
{
    private MouseState _previousMouseState;
    internal Action OnClose { get; init; } = () => { };
    internal Vector2 CameraOffset { get; private set; }
    internal bool IsPanning { get; private set; }

    internal override void Update()
    {
        base.Update();

        if (WasPressedThisFrame(Keys.Tab)) OnClose();

        var mouseState = Mouse.GetState();
        var gamepadState = GamePad.GetState(0);

        IsPanning = mouseState.MiddleButton == ButtonState.Pressed;
        if (IsPanning)
        {
            var mouseDelta = new Vector2(
                mouseState.X - _previousMouseState.X,
                mouseState.Y - _previousMouseState.Y
            );
            CameraOffset += mouseDelta;
        }

        // Handle panning with gamepad right thumbstick
        var thumbstickInput = gamepadState.ThumbSticks.Right;
        if (thumbstickInput.LengthSquared() > 0.02f)
            CameraOffset += new Vector2(-thumbstickInput.X, thumbstickInput.Y) * 8f;

        _previousMouseState = mouseState;
    }
}