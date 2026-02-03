using System.Linq;

namespace Gameplay.Background;

internal record DecalPalette
{
    public DecalPalette()
    {
        Decals =
        [
            new DecalDefinition(BackgroundDecal.Branch, 0.5f),
            new DecalDefinition(BackgroundDecal.MushroomBig1, 0.5f, AllowFlipX: false),
            new DecalDefinition(BackgroundDecal.MushroomBig2, 0.5f, AllowFlipX: false),
            new DecalDefinition(BackgroundDecal.MushroomCluster, 1f),
            new DecalDefinition(BackgroundDecal.Plant1, 1f),
            new DecalDefinition(BackgroundDecal.Plant2, 1f),
            new DecalDefinition(BackgroundDecal.Stump, 0.25f, AllowFlipX: false),
            new DecalDefinition(BackgroundDecal.RockMossy, 5f, DecalTheme.Rocks),
            new DecalDefinition(BackgroundDecal.Rocks1, 10f, DecalTheme.Rocks, false),
            new DecalDefinition(BackgroundDecal.Rocks2, 10f, DecalTheme.Rocks, false),
            new DecalDefinition(BackgroundDecal.Rocks3, 10f, DecalTheme.Rocks),
            new DecalDefinition(BackgroundDecal.Rocks4, 10f, DecalTheme.Rocks),
            new DecalDefinition(BackgroundDecal.Rocks5, 10f, DecalTheme.Rocks),
            new DecalDefinition(BackgroundDecal.Tuft1, 20f, DecalTheme.Tuft),
            new DecalDefinition(BackgroundDecal.Tuft2, 20f, DecalTheme.Tuft),
            new DecalDefinition(BackgroundDecal.Tuft3, 20f, DecalTheme.Tuft),
            new DecalDefinition(BackgroundDecal.Tuft4, 20f, DecalTheme.Tuft),
            new DecalDefinition(BackgroundDecal.TuftDark1, 20f, DecalTheme.TuftDark),
            new DecalDefinition(BackgroundDecal.TuftDark2, 20f, DecalTheme.TuftDark),
            new DecalDefinition(BackgroundDecal.TuftDark3, 20f, DecalTheme.TuftDark),
            new DecalDefinition(BackgroundDecal.TuftDark4, 20f, DecalTheme.TuftDark),
        ];
        TotalWeight = Decals.Sum(d => d.Weight);
    }

    internal DecalDefinition[] Decals { get; }
    internal float TotalWeight { get; }
}

internal readonly record struct DecalDefinition(
    BackgroundDecal Decal,
    float Weight,
    DecalTheme Theme = DecalTheme.None,
    bool AllowFlipX = true);

internal enum DecalTheme
{
    None,
    Tuft,
    TuftDark,
    Rocks,
}