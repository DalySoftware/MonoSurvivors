using System;
using System.Collections.Generic;
using System.Linq;
using GameLoop.UI;
using Gameplay.Levelling.PowerUps;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Title;

internal sealed class WeaponCarousel : IUiElement
{
    private readonly IUiElement _carousel;
    private readonly Button _previous;
    private readonly Button _next;

    public WeaponCarousel(IUiElement carousel, Button previous, Button next)
    {
        _carousel = carousel;
        _previous = previous;
        _next = next;

        Rectangle = ComputeRectangle();
    }

    public UiRectangle Rectangle { get; }

    public void Draw(SpriteBatch spriteBatch)
    {
        _carousel.Draw(spriteBatch);
        _previous.Draw(spriteBatch);
        _next.Draw(spriteBatch);
    }

    private UiRectangle ComputeRectangle()
    {
        var topLeftMost = new Vector2(float.MaxValue);
        var bottomRightMost = new Vector2(float.MinValue);

        foreach (var element in new[] { _carousel, _previous, _next })
        {
            var topLeft = element.Rectangle.AnchorForPoint(UiAnchor.TopLeft);
            var bottomRight = element.Rectangle.AnchorForPoint(UiAnchor.BottomRight);

            topLeftMost = Vector2.Min(topLeftMost, topLeft);
            bottomRightMost = Vector2.Max(bottomRightMost, bottomRight);
        }

        var size = bottomRightMost - topLeftMost;
        return new UiRectangle(topLeftMost, size);
    }
}

internal sealed class WeaponCarouselPanel : IUiElement
{
    private readonly List<IWeaponPanel> _panels;
    private int _currentIndex;

    public WeaponCarouselPanel(WeaponPanelFactory panelFactory, UiRectangle centerRect)
    {
        // Create panels for each weapon
        _panels = PowerUpCatalog.Weapons.Select(descriptor => panelFactory.Create(descriptor, centerRect)).ToList();

        _currentIndex = 0;
    }

    public WeaponDescriptor CurrentWeapon => _panels[_currentIndex].Descriptor;

    public UiRectangle Rectangle => _panels[_currentIndex].Rectangle;

    public void Draw(SpriteBatch spriteBatch) => _panels[_currentIndex].Draw(spriteBatch);

    public void Next() => _currentIndex = (_currentIndex + 1) % _panels.Count;

    public void Previous() => _currentIndex = (_currentIndex - 1 + _panels.Count) % _panels.Count;
}

internal interface IWeaponPanel : IUiElement
{
    WeaponDescriptor Descriptor { get; }
}

internal sealed class WeaponPanel(Panel panel, Texture2D icon, WeaponDescriptor descriptor) : IWeaponPanel
{
    public WeaponDescriptor Descriptor { get; } = descriptor;
    public UiRectangle Rectangle => panel.Rectangle;

    public void Draw(SpriteBatch spriteBatch)
    {
        panel.Draw(spriteBatch);

        var interior = panel.Interior;
        var iconSize = new Vector2(icon.Width, icon.Height);
        var iconTopLeft = interior.Centre - iconSize / 2f;

        spriteBatch.Draw(icon, iconTopLeft, layerDepth: panel.InteriorLayerDepth + 0.01f);
    }
}

internal sealed class WeaponPanelFactory(Panel.Factory panelFactory, PowerUpIcons powerUpIcons)
{
    /// <summary>
    ///     Create a WeaponPanel for a specific weapon type T.
    /// </summary>
    public IWeaponPanel Create(WeaponDescriptor descriptor, UiRectangle rectangle)
    {
        var icon = powerUpIcons.IconFor(descriptor.Unlock) ?? throw new Exception("Unknown weapon type");

        var interior = new UiRectangle(rectangle.Centre, rectangle.Size, UiAnchor.Centre);
        var panel = panelFactory.DefineByInterior(interior);

        return new WeaponPanel(panel, icon, descriptor);
    }
}