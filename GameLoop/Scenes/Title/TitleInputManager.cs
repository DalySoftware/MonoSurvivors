using System.Linq;
using GameLoop.Input;
using GameLoop.Rendering;
using GameLoop.UI;
using GameLoop.UserSettings;
using Gameplay;
using Microsoft.Xna.Framework;

namespace GameLoop.Scenes.Title;

internal sealed class TitleInputManager(
    IGlobalCommands globalCommands,
    IAppLifeCycle appLifeCycle,
    GameInputState inputState,
    KeyBindingsSettings bindings,
    WeaponSelect weaponSelect,
    IDisplayModeManager displayMode)
{
    private readonly TitleActionInput _actions = new(inputState, bindings);

    private Button? _hovered;
    private Button? _pressed;
    private Button? _focused;

    private InputMethod _lastInputMethod;

    private InputMethod CurrentInputMethod => inputState.CurrentInputMethod;

    internal void Update(GameTime gameTime)
    {
        // Handle switching between input methods
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

        // --- Global StartGame shortcut
        if (_actions.WasPressed(TitleAction.StartGame))
        {
            globalCommands.StartGame(weaponSelect.CurrentWeapon);
            return;
        }

        if (_actions.WasPressed(TitleAction.Exit) && appLifeCycle.CanExit)
        {
            appLifeCycle.Exit();
            return;
        }

        if (_actions.WasPressed(TitleAction.ToggleFullscreen))
            displayMode.ToggleFullscreen();

        if (_actions.WasPressed(TitleAction.NextWeapon))
            weaponSelect.NextButton.Activate();

        if (_actions.WasPressed(TitleAction.PreviousWeapon))
            weaponSelect.PreviousButton.Activate();

        // Mouse input
        if (CurrentInputMethod == InputMethod.KeyboardMouse)
            HandleMouseNavigation();

        if (CurrentInputMethod == InputMethod.Gamepad)
            HandleThumbstickNavigation(gameTime);
    }
    private void HandleThumbstickNavigation(GameTime gameTime)
    {
        var navigationAction = _actions.GetThumbstickNavigation(gameTime);
        switch (navigationAction)
        {
            case TitleAction.NextWeapon:
                weaponSelect.NextButton.Activate();
                break;
            case TitleAction.PreviousWeapon:
                weaponSelect.PreviousButton.Activate();
                break;
        }
    }

    private void HandleMouseNavigation()
    {
        var pointerPos = _actions.GetPointerPosition();
        if (pointerPos == null) return;

        var pressedThisFrame = _actions.WasLeftMousePressedThisFrame();
        var releasedThisFrame = _actions.WasLeftMouseReleasedThisFrame();

        var hovered = weaponSelect.Buttons.FirstOrDefault(b => b.Rectangle.Contains(pointerPos.Value));

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

    internal class Factory(
        IGlobalCommands globalCommands,
        IAppLifeCycle appLifeCycle,
        GameInputState inputState,
        KeyBindingsSettings bindings,
        IDisplayModeManager displayMode)
    {
        internal TitleInputManager Create(WeaponSelect weaponSelect) =>
            new(globalCommands, appLifeCycle, inputState, bindings, weaponSelect, displayMode);
    }
}