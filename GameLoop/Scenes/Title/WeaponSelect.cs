using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using GameLoop.UI;
using Gameplay.Levelling.PowerUps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Title;

internal sealed class WeaponSelect : IUiElement
{
    private readonly WeaponCarouselPanel _carousel;
    private readonly VerticalStack _stack;
    private readonly Label _titleLabel;
    private readonly Label _descriptionLabel;

    private WeaponSelect(
        Button.Factory buttonFactory,
        WeaponPanelFactory weaponPanelFactory,
        Label.Factory labelFactory,
        Vector2 topCenter)
    {
        // Have to tell the compiler we're assigning required props
        WeaponCarouselPanel carousel = null!;
        Button previousButton = null!;
        Button nextButton = null!;

        _stack = new VerticalStack(topCenter, 12f);
        _stack.AddChild(origin =>
        {
            var carouselRect = new UiRectangle(origin, new Vector2(100, 100), UiAnchor.TopCenter);
            carousel = new WeaponCarouselPanel(weaponPanelFactory, carouselRect);

            var padding = new Vector2(50f, 0f);

            previousButton = buttonFactory.Create(
                "<",
                () =>
                {
                    carousel.Previous();
                    RefreshLabels();
                },
                carouselRect.AnchorForPoint(UiAnchor.CenterLeft) - padding,
                UiAnchor.CenterRight,
                true);

            nextButton = buttonFactory.Create(
                ">",
                () =>
                {
                    carousel.Next();
                    RefreshLabels();
                },
                carouselRect.AnchorForPoint(UiAnchor.CenterRight) + padding,
                UiAnchor.CenterLeft,
                true);

            return new WeaponCarousel(carousel, previousButton, nextButton);
        });

        _carousel = carousel;
        PreviousButton = previousButton;
        NextButton = nextButton;

        _titleLabel = _stack.AddChild(origin =>
            labelFactory.Create(
                Paths.Fonts.BoldPixels.Medium,
                _carousel.CurrentWeapon.DisplayName,
                origin,
                UiAnchor.TopCenter,
                alignment: TextAlignment.Center,
                templateString: LongestName()));

        _descriptionLabel = _stack.AddChild(origin =>
            labelFactory.Create(
                Paths.Fonts.BoldPixels.Medium,
                _carousel.CurrentWeapon.Description,
                origin,
                UiAnchor.TopCenter,
                alignment: TextAlignment.Center,
                templateString: LongestDescription()));

        Rectangle = _stack.Rectangle;
    }

    internal Button PreviousButton { get; }
    internal Button NextButton { get; }

    internal IEnumerable<Button> Buttons => [PreviousButton, NextButton];
    internal WeaponDescriptor CurrentWeapon => _carousel.CurrentWeapon;

    public UiRectangle Rectangle { get; }

    public void Draw(SpriteBatch spriteBatch) => _stack.Draw(spriteBatch);

    private void RefreshLabels()
    {
        _titleLabel.Text = _carousel.CurrentWeapon.DisplayName;
        _descriptionLabel.Text = _carousel.CurrentWeapon.Description;
    }

    private static string LongestName() => PowerUpCatalog.Weapons.Max(w => w.DisplayName) ?? "";
    private static string LongestDescription() => PowerUpCatalog.Weapons.Max(w => w.Description) ?? "";

    /// <summary>
    ///     Factory for WeaponSelect
    /// </summary>
    internal sealed class Factory(
        Button.Factory buttonFactory,
        WeaponPanelFactory panelFactory,
        Label.Factory labelFactory)
    {
        internal WeaponSelect Create(Vector2 topCenter)
            => new(buttonFactory, panelFactory, labelFactory, topCenter);
    }
}