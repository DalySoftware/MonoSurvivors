using System;
using System.Linq;
using GameLoop.Input;
using GameLoop.Scenes.Pause.UI;
using GameLoop.UI;
using Gameplay;
using Microsoft.Xna.Framework;

namespace GameLoop.Scenes.Pause;

internal class PauseInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    PauseUi ui,
    SceneManager sceneManager)
    : BaseInputManager(globalCommands, inputState, sceneManager)
{
    private readonly PauseActionInput _actions = new(inputState);

    private readonly Action _onResume = globalCommands.ResumeGame;

    private Button? _hovered;
    private Button? _pressed;
    private Button? _focused;

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
        if (_actions.WasPressed(PauseAction.Resume))
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
        var buttons = ui.Buttons.ToList();

        if (_focused == null)
        {
            _focused = buttons[0];
            _focused.Focus();
        }

        var navDirection = _actions.GetNavigationDirection(gameTime);

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

        var activate = _actions.WasPressed(PauseAction.Activate);
        if (activate && _focused != null) _focused.Activate();
    }

    private void HandleMouseNavigation()
    {
        if (_actions.GetPointerPosition() is not { } pointerPos) return;

        var pressedThisFrame = _actions.WasLeftMousePressedThisFrame();
        var releasedThisFrame = _actions.WasLeftMouseReleasedThisFrame();

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
}