using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using GameLoop.UI;
using Gameplay.Stats;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Stats;

internal sealed class RunStatsPanel : IUiElement
{
    private readonly static (string name, bool elite, string label)[] EnemyOrder =
    [
        ("Skug", false, "Skug"),
        ("Skug", true, "Elite Skug"),
        ("Scorcher", false, "Scorcher"),
        ("Scorcher", true, "Elite Scorcher"),
        ("Hulker", false, "Hulker"),
        ("Hulker", true, "Elite Hulker"),
        ("Jorgie", false, "Jorgie"),
    ];
    private readonly Label _title;

    private readonly VerticalStack _mainStack;

    private readonly Label _killsTitle;
    private readonly VerticalStack? _killsCenter;
    private readonly VerticalStack? _killsLeft;
    private readonly VerticalStack? _killsRight;

    public RunStatsPanel(
        Vector2 topCenter,
        Label.Factory labelFactory,
        StatsCounter stats)
    {
        _title = labelFactory.Create(Paths.Fonts.BoldPixels.Large, "Statistics", topCenter, UiAnchor.TopCenter);

        var mainStackStart = _title.Rectangle.AnchorForPoint(UiAnchor.BottomCenter) + new Vector2(0f, 20f);

        _mainStack = new VerticalStack(mainStackStart, 20);
        AddLine(_mainStack, $"Damage Dealt: {stats.DamageDealt:0}", 25);
        AddLine(_mainStack, $"Damage Taken: {stats.DamageTaken}", 25);
        AddLine(_mainStack, $"Healing Received: {stats.HealingReceived}", 25);
        AddLine(_mainStack, $"Experience Gained: {stats.ExperienceGained:0}", 25);

        var killsTitlePosition = _mainStack.Rectangle.AnchorForPoint(UiAnchor.BottomCenter) + new Vector2(0f, 30f);
        _killsTitle = labelFactory.Create(Paths.Fonts.BoldPixels.Medium, "Enemies Killed", killsTitlePosition,
            UiAnchor.TopCenter);

        var lines = BuildEnemyKillLines(stats);

        var enemyKillsStart = _killsTitle.Rectangle.AnchorForPoint(UiAnchor.BottomCenter) + new Vector2(0f, 20f);
        if (lines.Length <= 4)
        {
            _killsCenter = new VerticalStack(enemyKillsStart, 20);
            _killsLeft = null;
            _killsRight = null;

            foreach (var line in lines)
                AddLine(_killsCenter, line, 20);

            return;
        }

        _killsCenter = null;
        _killsLeft = new VerticalStack(enemyKillsStart + new Vector2(-150f, 0f), 20);
        _killsRight = new VerticalStack(enemyKillsStart + new Vector2(150f, 0f), 20);

        var split = (lines.Length + 1) / 2;
        for (var i = 0; i < lines.Length; i++)
            AddLine(i < split ? _killsLeft : _killsRight, lines[i], 20);

        return;

        void AddLine(VerticalStack stack, string text, int padLength = 0) =>
            stack.AddChild(pos => labelFactory.Create(Paths.Fonts.BoldPixels.Medium, text, pos, UiAnchor.TopCenter,
                templateString: string.Join(string.Empty, Enumerable.Repeat("x", padLength))));
    }

    public UiRectangle Rectangle => _title.Rectangle;

    private static string[] BuildEnemyKillLines(StatsCounter stats)
    {
        var killCounts = CountKills(stats);

        var lines = EnemyOrder
            .Select(spec => (spec.label, count: killCounts.GetValueOrDefault((spec.name, spec.elite))))
            .Where(x => x.count > 0)
            .Select(x => $"{x.label}: {x.count}")
            .ToArray();
        return lines;
    }

    private static Dictionary<(string name, bool elite), int> CountKills(StatsCounter stats)
    {
        var killCounts = new Dictionary<(string name, bool elite), int>();
        foreach (var ((enemyType, elite), value) in stats.Kills)
        {
            var name = enemyType.Name;

            killCounts[(name, elite)] = killCounts.GetValueOrDefault((name, elite)) + value;
        }

        return killCounts;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _title.Draw(spriteBatch);
        _mainStack.Draw(spriteBatch);

        _killsTitle.Draw(spriteBatch);
        _killsCenter?.Draw(spriteBatch);
        _killsLeft?.Draw(spriteBatch);
        _killsRight?.Draw(spriteBatch);
    }
}