using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering.Tooltips;

public record ToolTip(string Title, IReadOnlyCollection<ToolTipBodyLine> Body)
{
    internal int TotalLines => Body.Count + 1; // Add 1 for title

    internal float MaxWidth(SpriteFont font)
    {
        var titleWidth = font.MeasureString(Title).X;
        var bodyWidth = Body
            .Select(line => font.MeasureString(line.Text).X)
            .DefaultIfEmpty(0f)
            .Max();

        return Math.Max(titleWidth, bodyWidth);
    }
}

public record ToolTipBodyLine(string Text, Color? Color = null);