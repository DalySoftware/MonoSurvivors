using System;
using System.Collections.Generic;
using System.Linq;
using GameLoop.UserSettings;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input.Mapping;

public sealed class ActionKeyMap<TAction> : IMergeable<ActionKeyMap<TAction>> where TAction : Enum
{
    public Dictionary<TAction, List<Keys>> Keyboard { get; set; } = new();
    public Dictionary<TAction, List<Buttons>> Gamepad { get; set; } = new();

    public void MergeFrom(ActionKeyMap<TAction> overrides)
    {
        foreach (var (action, keys) in overrides.Keyboard)
            Keyboard[action] = keys;

        foreach (var (action, buttons) in overrides.Gamepad)
            Gamepad[action] = buttons;
    }

    private static ActionKeyMap<T> Clone<T>(ActionKeyMap<T> source) where T : Enum => new()
    {
        Keyboard = source.Keyboard.ToDictionary(kvp => kvp.Key, kvp => new List<Keys>(kvp.Value)),

        Gamepad = source.Gamepad.ToDictionary(kvp => kvp.Key, kvp => new List<Buttons>(kvp.Value)),
    };


    public IReadOnlyList<Keys> GetKeys(TAction action) =>
        Keyboard.TryGetValue(action, out var keys) ? keys : [];

    public IReadOnlyList<Buttons> GetButtons(TAction action) =>
        Gamepad.TryGetValue(action, out var buttons) ? buttons : [];
}