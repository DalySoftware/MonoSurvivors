using System;
using System.Collections.Generic;
using System.Linq;
using GameLoop.UI;
using Gameplay.Levelling.PowerUps;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Title;

internal sealed class WeaponSelect : IUiElement
{
    private readonly WeaponPanelCarousel _carousel;

    private WeaponSelect(
        Button.Factory buttonFactory,
        WeaponPanelFactory weaponPanelFactory,
        Vector2 topCenter)
    {
        // --- Central carousel
        var carouselRect = new UiRectangle(topCenter, new Vector2(100, 100), UiAnchor.TopCenter);

        _carousel = new WeaponPanelCarousel(weaponPanelFactory, carouselRect);

        var buttonPadding = new Vector2(50f, 0f);

        PreviousButton = buttonFactory.Create("<", _carousel.Previous,
            _carousel.Rectangle.AnchorForPoint(UiAnchor.CenterLeft) - buttonPadding, UiAnchor.CenterRight,
            true);

        NextButton = buttonFactory.Create(
            ">",
            _carousel.Next,
            _carousel.Rectangle.AnchorForPoint(UiAnchor.CenterRight) + buttonPadding, UiAnchor.CenterLeft,
            true);

        var height = Elements.Max(e => e.Rectangle.Size.Y);
        var minX = Elements.Min(e => e.Rectangle.TopLeft.X);
        var maxX = Elements.Max(e => e.Rectangle.AnchorForPoint(UiAnchor.CenterRight).X);

        var size = new Vector2(maxX - minX, height);
        Rectangle = new UiRectangle(topCenter, size, UiAnchor.TopCenter);
    }

    internal Button PreviousButton { get; }
    internal Button NextButton { get; }

    internal IEnumerable<Button> Buttons => [PreviousButton, NextButton];
    private IEnumerable<IUiElement> Elements => [_carousel, ..Buttons];
    internal WeaponDescriptor CurrentWeapon => _carousel.CurrentWeapon;

    public UiRectangle Rectangle { get; }

    public void Draw(SpriteBatch spriteBatch)
    {
        _carousel.Draw(spriteBatch);
        PreviousButton.Draw(spriteBatch);
        NextButton.Draw(spriteBatch);
    }

    /// <summary>
    ///     Factory for WeaponSelect
    /// </summary>
    internal sealed class Factory(Button.Factory buttonFactory, WeaponPanelFactory panelFactory)
    {
        internal WeaponSelect Create(Vector2 topCenter)
            => new(buttonFactory, panelFactory, topCenter);
    }
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

internal sealed class WeaponPanelFactory(
    Panel.Factory panelFactory,
    PowerUpIcons powerUpIcons)
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

internal sealed class WeaponPanelCarousel : IUiElement
{
    private readonly List<IWeaponPanel> _panels;

    public WeaponPanelCarousel(WeaponPanelFactory panelFactory, UiRectangle centerRect)
    {
        // Create panels for each weapon
        _panels = PowerUpCatalog.Weapons.Select(descriptor => panelFactory.Create(descriptor, centerRect)).ToList();

        CurrentIndex = 0;
    }

    public int CurrentIndex { get; private set; }

    public WeaponDescriptor CurrentWeapon => _panels[CurrentIndex].Descriptor;

    public UiRectangle Rectangle => _panels[CurrentIndex].Rectangle;

    public void Draw(SpriteBatch spriteBatch) => _panels[CurrentIndex].Draw(spriteBatch);

    public void Next() => CurrentIndex = (CurrentIndex + 1) % _panels.Count;

    public void Previous() => CurrentIndex = (CurrentIndex - 1 + _panels.Count) % _panels.Count;
}