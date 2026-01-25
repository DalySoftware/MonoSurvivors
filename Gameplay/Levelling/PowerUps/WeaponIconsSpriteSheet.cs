using ContentLibrary;
using Gameplay.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Levelling.PowerUps;

public class WeaponIconsSpriteSheet(ContentManager content)
{
    private const string DataJson = """
                                    { "frames": [
                                       {
                                        "filename": "Bouncer.ase",
                                        "frame": { "x": 5, "y": 5, "w": 128, "h": 128 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 128, "h": 128 },
                                        "sourceSize": { "w": 128, "h": 128 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Enforcer.ase",
                                        "frame": { "x": 5, "y": 139, "w": 128, "h": 128 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 128, "h": 128 },
                                        "sourceSize": { "w": 128, "h": 128 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Ice Aura.ase",
                                        "frame": { "x": 5, "y": 273, "w": 128, "h": 128 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 128, "h": 128 },
                                        "sourceSize": { "w": 128, "h": 128 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Shotgun.ase",
                                        "frame": { "x": 5, "y": 407, "w": 128, "h": 128 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 128, "h": 128 },
                                        "sourceSize": { "w": 128, "h": 128 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Sniper.ase",
                                        "frame": { "x": 5, "y": 541, "w": 128, "h": 128 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 128, "h": 128 },
                                        "sourceSize": { "w": 128, "h": 128 },
                                        "duration": 100
                                       }
                                     ],
                                     "meta": {
                                      "app": "https://www.aseprite.org/",
                                      "version": "1.3.16.1-x64",
                                      "image": "Weapons.png",
                                      "format": "RGBA8888",
                                      "size": { "w": 138, "h": 674 },
                                      "scale": "1"
                                     }
                                    }
                                    """;

    private readonly AsepriteAtlas _atlas = new(content.Load<Texture2D>(Paths.Images.Sheets.WeaponIcons), DataJson);

    internal SpriteFrame GetFrameRectangle(IPowerUp powerUp) => _atlas.GetFrame(powerUp.Title());
}