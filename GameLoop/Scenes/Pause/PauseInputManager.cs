using System;
using System.Linq;
using GameLoop.Input;
using GameLoop.Scenes.Pause.UI;
using GameLoop.UI;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Pause;

internal class PauseInputManager(IGlobalCommands globalCommands, PauseUi ui) : BaseInputManager
{
    private readonly Action _onResume = globalCommands.ResumeGame;

    private Button? _hovered;
    private Button? _pressed;

    internal override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // --- Global resume shortcuts
        if (WasPressedThisFrame(Keys.Escape) ||
            WasPressedThisFrame(Buttons.Start) ||
            WasPressedThisFrame(Buttons.B))
        {
            _onResume();
            return;
        }

        var mouse = Mouse.GetState();
        var pointerPos = mouse.Position;
        var down = mouse.LeftButton == ButtonState.Pressed;
        var pressedThisFrame = down && WasLeftMousePressedThisFrame();
        var releasedThisFrame = !down && WasLeftMouseReleasedThisFrame();

        var hovered = ui.Buttons.FirstOrDefault(b => b.Bounds.Contains(pointerPos));

        if (hovered != _hovered)
        {
            _hovered?.Unhover();
            hovered?.Hover();
            _hovered = hovered;
        }

        if (pressedThisFrame && _hovered != null)
        {
            _pressed = _hovered;
            _pressed.PressVisual();
        }

        if (releasedThisFrame)
        {
            _pressed?.ReleaseVisual();

            if (_pressed != null && _pressed == _hovered)
                _pressed.Activate();

            _pressed = null;
        }
    }

    private bool WasLeftMouseReleasedThisFrame() => MouseState.LeftButton == ButtonState.Released &&
                                                    PreviousMouseState.LeftButton == ButtonState.Pressed;
    private bool WasLeftMousePressedThisFrame() => MouseState.LeftButton == ButtonState.Pressed &&
                                                   PreviousMouseState.LeftButton == ButtonState.Released;
}