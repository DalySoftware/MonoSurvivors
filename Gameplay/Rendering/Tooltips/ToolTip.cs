using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Rendering.Tooltips;

public record ToolTip(string Title, IReadOnlyCollection<ToolTipBodyLine> Body)
{
    // 1 for title
    internal int TotalLines => Body.Count + 1;
    
    internal float MaxWidth(SpriteFont font)
    {
        var titleWidth =  font.MeasureString(Title).X;
        var bodyWidth = Body.Max(line => font.MeasureString((string)line.Text).X);
        return Math.Max(titleWidth, bodyWidth);
    }
};

public record ToolTipBodyLine(string Text, Color? Color = null);