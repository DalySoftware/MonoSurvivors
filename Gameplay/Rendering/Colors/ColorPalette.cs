namespace Gameplay.Rendering.Colors;

public static class ColorPalette
{
    public static Color Black { get; } = new(0, 0, 0); // RGB_BLACK
    public static Color Night { get; } = new(34, 32, 52); // RGB_NIGHT
    public static Color Wine { get; } = new(69, 40, 60); // RGB_WINE
    public static Color Brown { get; } = new(102, 57, 49); // RGB_BROWN
    public static Color Coconut { get; } = new(143, 86, 59); // RGB_COCONUT
    public static Color Orange { get; } = new(223, 113, 38); // RGB_ORANGE
    public static Color Tan { get; } = new(217, 160, 102); // RGB_TAN
    public static Color Peach { get; } = new(238, 195, 154); // was GOLD
    public static Color Yellow { get; } = new(251, 242, 54); // RGB_YELLOW
    public static Color Lime { get; } = new(153, 229, 80); // was KIWI
    public static Color Green { get; } = new(106, 190, 48); // RGB_GREEN
    public static Color Teal { get; } = new(55, 148, 110); // was EMERALD
    public static Color Olive { get; } = new(75, 105, 47); // RGB_OLIVE
    public static Color Camo { get; } = new(82, 75, 36); // RGB_CAMO
    public static Color Charcoal { get; } = new(50, 60, 57); // was ONYX
    public static Color Navy { get; } = new(63, 63, 116); // RGB_NAVY
    public static Color Agave { get; } = new(48, 96, 130); // RGB_AGAVE
    public static Color Royal { get; } = new(91, 110, 225); // RGB_ROYAL
    public static Color Blue { get; } = new(99, 155, 255); // RGB_BLUE
    public static Color Cyan { get; } = new(95, 205, 228); // RGB_CYAN
    public static Color Ice { get; } = new(203, 219, 252); // RGB_ICE
    public static Color White { get; } = new(255, 255, 255); // RGB_WHITE

    // Neutral grays
    public static Color LightGray { get; } = new(155, 173, 183); // RGB_SMOKE
    public static Color Gray { get; } = new(105, 106, 106); // RGB_DIM
    public static Color DarkGray { get; } = new(89, 86, 82); // RGB_GRAY

    // Accents
    public static Color Violet { get; } = new(118, 66, 138); // RGB_VIOLET
    public static Color Red { get; } = new(172, 50, 50); // RGB_RED
    public static Color Pink { get; } = new(217, 87, 99); // RGB_PINK
    public static Color Candy { get; } = new(215, 123, 186); // RGB_CANDY
    public static Color Moss { get; } = new(143, 151, 74); // RGB_MOSS
    public static Color Umber { get; } = new(138, 111, 48); // was BISTRE

    public static Color ItchBranding { get; } = new(4, 2, 3);
}