using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input.Mapping;

public sealed class ActionKeyMap<TAction> where TAction : Enum
{
    public Dictionary<TAction, List<Keys>> Keyboard { get; set; } = new();
    public Dictionary<TAction, List<Buttons>> Gamepad { get; set; } = new();
}