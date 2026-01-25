using ContentLibrary;
using Gameplay.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Levelling.PowerUps;

public sealed class PowerUpIconSpriteSheet(ContentManager content)
{
    private const string DataJson = """
                                    { "frames": [
                                       {
                                        "filename": "Attack Speed.ase",
                                        "frame": { "x": 5, "y": 5, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Bullet Split.ase",
                                        "frame": { "x": 5, "y": 75, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Chain Lightning.ase",
                                        "frame": { "x": 5, "y": 145, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Critical Hit Chance.ase",
                                        "frame": { "x": 5, "y": 215, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Critical Hit Damage.ase",
                                        "frame": { "x": 5, "y": 285, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Damage.ase",
                                        "frame": { "x": 5, "y": 355, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Dodge Chance.ase",
                                        "frame": { "x": 5, "y": 425, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Experience Multiplier.ase",
                                        "frame": { "x": 5, "y": 495, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Explode On Kill.ase",
                                        "frame": { "x": 5, "y": 565, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Extra Shot Chance.ase",
                                        "frame": { "x": 5, "y": 635, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Grid Vision.ase",
                                        "frame": { "x": 5, "y": 705, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Health Regen.ase",
                                        "frame": { "x": 5, "y": 775, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Life Steal.ase",
                                        "frame": { "x": 5, "y": 845, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Max Health.ase",
                                        "frame": { "x": 5, "y": 915, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Pickup Radius.ase",
                                        "frame": { "x": 5, "y": 985, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Pierce.ase",
                                        "frame": { "x": 5, "y": 1055, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Projectile Speed.ase",
                                        "frame": { "x": 5, "y": 1125, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Range.ase",
                                        "frame": { "x": 5, "y": 1195, "w": 64, "h": 64 },
                                        "rotated": false,
                                        "trimmed": false,
                                        "spriteSourceSize": { "x": 0, "y": 0, "w": 64, "h": 64 },
                                        "sourceSize": { "w": 64, "h": 64 },
                                        "duration": 100
                                       },
                                       {
                                        "filename": "Speed.ase",
                                        "frame": { "x": 5, "y": 1265, "w": 64, "h": 64 },
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
                                      "image": "PowerUpIcons.png",
                                      "format": "RGBA8888",
                                      "size": { "w": 74, "h": 1334 },
                                      "scale": "1"
                                     }
                                    }
                                    """;

    private readonly AsepriteAtlas _atlas = new(content.Load<Texture2D>(Paths.Images.Sheets.PowerUpIcons), DataJson);

    internal SpriteFrame GetFrameRectangle(IPowerUp powerUp) => _atlas.GetFrame(powerUp.Title());
}