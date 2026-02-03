using ContentLibrary;
using Gameplay.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Background;

public class BackgroundDecalSpriteSheet(ContentManager content)
{
    private const string DataJson = """
                                    { "frames": [
                                       {
                                        "filename": "Branch.ase",
                                        "frame": { "x": 5, "y": 5, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "MushroomBig1.ase",
                                        "frame": { "x": 5, "y": 75, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "MushroomBig2.ase",
                                        "frame": { "x": 5, "y": 145, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "MushroomCluster.ase",
                                        "frame": { "x": 5, "y": 215, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Plant1.ase",
                                        "frame": { "x": 5, "y": 285, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Plant2.ase",
                                        "frame": { "x": 5, "y": 355, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "RockMossy.ase",
                                        "frame": { "x": 5, "y": 425, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Rocks1.ase",
                                        "frame": { "x": 5, "y": 495, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Rocks2.ase",
                                        "frame": { "x": 5, "y": 565, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Rocks3.ase",
                                        "frame": { "x": 5, "y": 635, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Rocks4.ase",
                                        "frame": { "x": 5, "y": 705, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Rocks5.ase",
                                        "frame": { "x": 5, "y": 775, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Stump.ase",
                                        "frame": { "x": 5, "y": 845, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Tuft1.ase",
                                        "frame": { "x": 5, "y": 915, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Tuft2.ase",
                                        "frame": { "x": 5, "y": 985, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Tuft3.ase",
                                        "frame": { "x": 5, "y": 1055, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Tuft4.ase",
                                        "frame": { "x": 5, "y": 1125, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "TuftDark1.ase",
                                        "frame": { "x": 5, "y": 1195, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "TuftDark2.ase",
                                        "frame": { "x": 5, "y": 1265, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "TuftDark3.ase",
                                        "frame": { "x": 5, "y": 1335, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "TuftDark4.ase",
                                        "frame": { "x": 5, "y": 1405, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       }
                                     ],
                                     "meta": {
                                      "app": "https://www.aseprite.org/",
                                      "version": "1.3.16.1-x64",
                                      "image": "BackgroundDecals.png",
                                      "format": "RGBA8888",
                                      "size": { "w": 74, "h": 1474 },
                                      "scale": "1"
                                     }
                                    }
                                    """;

    private readonly AsepriteAtlas
        _atlas = new(content.Load<Texture2D>(Paths.Images.Sheets.BackgroundDecals), DataJson);

    internal SpriteFrame GetFrameRectangle(BackgroundDecal powerUp) => _atlas.GetFrame(powerUp.ToString());
}

internal enum BackgroundDecal
{
    Branch,
    MushroomBig1,
    MushroomBig2,
    MushroomCluster,
    Plant1,
    Plant2,
    RockMossy,
    Rocks1,
    Rocks2,
    Rocks3,
    Rocks4,
    Rocks5,
    Stump,
    Tuft1,
    Tuft2,
    Tuft3,
    Tuft4,
    TuftDark1,
    TuftDark2,
    TuftDark3,
    TuftDark4,
}