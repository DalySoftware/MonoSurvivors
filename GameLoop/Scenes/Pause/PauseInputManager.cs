using System;
using System.Linq;
using GameLoop.Input;
using GameLoop.Scenes.Pause.UI;
using GameLoop.UI;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Pause;

internal class PauseInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    PauseUi ui,
    SceneManager sceneManager)
    : BaseInputManager(globalCommands, inputState, sceneManager)
{
    private readonly Action _onResume = globalCommands.ResumeGame;

    private Button? _hovered;
    private Button? _pressed;
    private Button? _focused;

    private TimeSpan _navCooldown = TimeSpan.Zero;
    private InputMethod _lastInputMethod;

    internal void Update(GameTime gameTime)
    {
        if (ShouldSkipInput()) return;

        if (CurrentInputMethod != _lastInputMethod)
        {
            // Leaving mouse mode
            if (_lastInputMethod == InputMethod.KeyboardMouse)
            {
                _hovered?.Unhover();
                _pressed?.ReleaseVisual();
                _hovered = null;
                _pressed = null;
            }

            // Leaving gamepad mode
            if (_lastInputMethod == InputMethod.Gamepad)
            {
                _focused?.Blur();
                _focused = null;
            }

            _lastInputMethod = CurrentInputMethod;
        }


        // --- Global resume shortcuts
        if (WasPressedThisFrame(Keys.Escape) ||
            WasPressedThisFrame(Buttons.Start) ||
            WasPressedThisFrame(Buttons.B))
        {
            _onResume();
            return;
        }

        if (CurrentInputMethod == InputMethod.KeyboardMouse)
            HandleMouseNavigation();

        if (CurrentInputMethod == InputMethod.Gamepad)
            HandleGamepadNavigation(gameTime);
    }

    private void HandleGamepadNavigation(GameTime gameTime)
    {
        _navCooldown -= gameTime.ElapsedGameTime;

        var activate = WasPressedThisFrame(Buttons.A);

        var buttons = ui.Buttons.ToList();

        if (_focused == null)
        {
            _focused = buttons[0];
            _focused.Focus();
        }

        var navDirection = GetThumbstickDirection() ?? GetDpadDirection();

        if (navDirection is { } direction)
        {
            var next = UiNavigator.FindNext(_focused, buttons, direction);
            if (next != null && next != _focused)
            {
                _focused.Blur();
                _focused = next;
                _focused.Focus();
            }
        }

        if (activate && _focused != null) _focused.Activate();
    }

    private Direction? GetDpadDirection()
    {
        if (WasPressedThisFrame(Buttons.DPadDown)) return Direction.Down;
        if (WasPressedThisFrame(Buttons.DPadUp)) return Direction.Up;
        if (WasPressedThisFrame(Buttons.DPadLeft)) return Direction.Left;
        if (WasPressedThisFrame(Buttons.DPadRight)) return Direction.Right;
        return null;
    }

    private Direction? GetThumbstickDirection()
    {
        if (_navCooldown > TimeSpan.Zero)
            return null;

        var x = InputState.GamePadState.ThumbSticks.Left.X;
        var y = InputState.GamePadState.ThumbSticks.Left.Y;

        Direction? direction = (x, y) switch
        {
            (_, > 0.5f) => Direction.Up,
            (_, < -0.5f) => Direction.Down,
            (< -0.5f, _) => Direction.Left,
            (> 0.5f, _) => Direction.Right,
            _ => null,
        };

        if (direction is not null)
            _navCooldown = TimeSpan.FromMilliseconds(150);

        return direction;
    }


    private void HandleMouseNavigation()
    {
        var pointerPos = InputState.MouseState.Position.ToVector2();
        var down = InputState.MouseState.LeftButton == ButtonState.Pressed;
        var pressedThisFrame = down && WasLeftMousePressedThisFrame();
        var releasedThisFrame = !down && WasLeftMouseReleasedThisFrame();

        var hovered = ui.Buttons.FirstOrDefault(b => b.Rectangle.Contains(pointerPos));

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

    private bool WasLeftMouseReleasedThisFrame() => InputState.MouseState.LeftButton == ButtonState.Released &&
                                                    InputState.PreviousMouseState.LeftButton == ButtonState.Pressed;
    private bool WasLeftMousePressedThisFrame() => InputState.MouseState.LeftButton == ButtonState.Pressed &&
                                                   InputState.PreviousMouseState.LeftButton == ButtonState.Released;
}