using System.Collections.Generic;
using Gameplay.Levelling.PowerUps;

// ReSharper disable RedundantEmptyObjectOrCollectionInitializer

namespace Gameplay.Levelling.SphereGrid;

public static class TemplateFactory
{
    internal static GridTemplate CreateTemplate() => new()
{
    RootId = 0,
    Nodes =
    [
        new NodeTemplate
        {
            Id = 0,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 102 },
                { EdgeDirection.BottomRight, 83 },
                { EdgeDirection.TopRight, 56 },
                { EdgeDirection.MiddleLeft, 39 },
                { EdgeDirection.MiddleRight, 20 },
                { EdgeDirection.BottomLeft, 1 },
            },
        },
        new NodeTemplate
        {
            Id = 1,
            Category = PowerUpCategory.Speed,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 40 },
            },
        },
        new NodeTemplate
        {
            Id = 2,
            Category = PowerUpCategory.DamageEffects,
            Rarity = NodeRarity.Rare,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 115 },
            },
        },
        new NodeTemplate
        {
            Id = 3,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 40 },
                { EdgeDirection.BottomRight, 4 },
            },
        },
        new NodeTemplate
        {
            Id = 4,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 11 },
                { EdgeDirection.BottomLeft, 5 },
            },
        },
        new NodeTemplate
        {
            Id = 5,
            Category = PowerUpCategory.Speed,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 10 },
                { EdgeDirection.BottomRight, 6 },
            },
        },
        new NodeTemplate
        {
            Id = 6,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 7 },
            },
        },
        new NodeTemplate
        {
            Id = 7,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 8 },
            },
        },
        new NodeTemplate
        {
            Id = 8,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 9 },
            },
        },
        new NodeTemplate
        {
            Id = 9,
            Category = PowerUpCategory.DamageEffects,
            Rarity = NodeRarity.Legendary,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 10,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 11,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 12 },
            },
        },
        new NodeTemplate
        {
            Id = 12,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 115 },
            },
        },
        new NodeTemplate
        {
            Id = 13,
            Category = PowerUpCategory.Health,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 39 },
                { EdgeDirection.BottomLeft, 14 },
                { EdgeDirection.MiddleLeft, 41 },
            },
        },
        new NodeTemplate
        {
            Id = 14,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 15 },
            },
        },
        new NodeTemplate
        {
            Id = 15,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 16 },
            },
        },
        new NodeTemplate
        {
            Id = 16,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 17 },
            },
        },
        new NodeTemplate
        {
            Id = 17,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 18 },
            },
        },
        new NodeTemplate
        {
            Id = 18,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 19 },
            },
        },
        new NodeTemplate
        {
            Id = 19,
            Category = PowerUpCategory.DamageEffects,
            Rarity = NodeRarity.Rare,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 20,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 21 },
            },
        },
        new NodeTemplate
        {
            Id = 21,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 22 },
            },
        },
        new NodeTemplate
        {
            Id = 22,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 23 },
            },
        },
        new NodeTemplate
        {
            Id = 23,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 24 },
            },
        },
        new NodeTemplate
        {
            Id = 24,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 32 },
                { EdgeDirection.TopLeft, 28 },
                { EdgeDirection.MiddleRight, 25 },
            },
        },
        new NodeTemplate
        {
            Id = 25,
            Category = PowerUpCategory.Speed,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 27 },
                { EdgeDirection.MiddleRight, 26 },
            },
        },
        new NodeTemplate
        {
            Id = 26,
            Category = PowerUpCategory.Health,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 27,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 28,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 29 },
            },
        },
        new NodeTemplate
        {
            Id = 29,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 30 },
            },
        },
        new NodeTemplate
        {
            Id = 30,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 31 },
            },
        },
        new NodeTemplate
        {
            Id = 31,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Rare,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 32,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 33 },
            },
        },
        new NodeTemplate
        {
            Id = 33,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 34 },
            },
        },
        new NodeTemplate
        {
            Id = 34,
            Category = PowerUpCategory.Speed,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 35 },
            },
        },
        new NodeTemplate
        {
            Id = 35,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 36 },
            },
        },
        new NodeTemplate
        {
            Id = 36,
            Category = PowerUpCategory.Health,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 37 },
            },
        },
        new NodeTemplate
        {
            Id = 37,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 38 },
            },
        },
        new NodeTemplate
        {
            Id = 38,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Legendary,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 39,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 40,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 41,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 42 },
            },
        },
        new NodeTemplate
        {
            Id = 42,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 52 },
                { EdgeDirection.MiddleLeft, 43 },
            },
        },
        new NodeTemplate
        {
            Id = 43,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 46 },
                { EdgeDirection.TopLeft, 44 },
            },
        },
        new NodeTemplate
        {
            Id = 44,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 45 },
            },
        },
        new NodeTemplate
        {
            Id = 45,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 46,
            Category = PowerUpCategory.Speed,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 47 },
            },
        },
        new NodeTemplate
        {
            Id = 47,
            Category = PowerUpCategory.Health,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 48 },
            },
        },
        new NodeTemplate
        {
            Id = 48,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 49 },
            },
        },
        new NodeTemplate
        {
            Id = 49,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 50 },
            },
        },
        new NodeTemplate
        {
            Id = 50,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 51 },
            },
        },
        new NodeTemplate
        {
            Id = 51,
            Category = PowerUpCategory.WeaponUnlock,
            Rarity = NodeRarity.Legendary,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 52,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 53 },
            },
        },
        new NodeTemplate
        {
            Id = 53,
            Category = PowerUpCategory.Health,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 54 },
            },
        },
        new NodeTemplate
        {
            Id = 54,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 55 },
            },
        },
        new NodeTemplate
        {
            Id = 55,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Rare,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 56,
            Category = PowerUpCategory.Health,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 63 },
                { EdgeDirection.TopLeft, 57 },
            },
        },
        new NodeTemplate
        {
            Id = 57,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 58 },
            },
        },
        new NodeTemplate
        {
            Id = 58,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 59 },
            },
        },
        new NodeTemplate
        {
            Id = 59,
            Category = PowerUpCategory.Health,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 60 },
            },
        },
        new NodeTemplate
        {
            Id = 60,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 61 },
            },
        },
        new NodeTemplate
        {
            Id = 61,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 62 },
            },
        },
        new NodeTemplate
        {
            Id = 62,
            Category = PowerUpCategory.DamageEffects,
            Rarity = NodeRarity.Rare,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 63,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 64 },
            },
        },
        new NodeTemplate
        {
            Id = 64,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 65 },
            },
        },
        new NodeTemplate
        {
            Id = 65,
            Category = PowerUpCategory.Speed,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 78 },
                { EdgeDirection.TopRight, 72 },
                { EdgeDirection.MiddleRight, 66 },
            },
        },
        new NodeTemplate
        {
            Id = 66,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 67 },
            },
        },
        new NodeTemplate
        {
            Id = 67,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 68 },
            },
        },
        new NodeTemplate
        {
            Id = 68,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 69 },
            },
        },
        new NodeTemplate
        {
            Id = 69,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 70 },
            },
        },
        new NodeTemplate
        {
            Id = 70,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 71 },
            },
        },
        new NodeTemplate
        {
            Id = 71,
            Category = PowerUpCategory.DamageEffects,
            Rarity = NodeRarity.Rare,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 72,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 73 },
            },
        },
        new NodeTemplate
        {
            Id = 73,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 74 },
            },
        },
        new NodeTemplate
        {
            Id = 74,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 75 },
            },
        },
        new NodeTemplate
        {
            Id = 75,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 76 },
            },
        },
        new NodeTemplate
        {
            Id = 76,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 77 },
            },
        },
        new NodeTemplate
        {
            Id = 77,
            Category = PowerUpCategory.DamageEffects,
            Rarity = NodeRarity.Legendary,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 78,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 79 },
            },
        },
        new NodeTemplate
        {
            Id = 79,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 80 },
            },
        },
        new NodeTemplate
        {
            Id = 80,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 81 },
            },
        },
        new NodeTemplate
        {
            Id = 81,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 82 },
            },
        },
        new NodeTemplate
        {
            Id = 82,
            Category = PowerUpCategory.Health,
            Rarity = NodeRarity.Rare,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 83,
            Category = PowerUpCategory.Speed,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 84 },
            },
        },
        new NodeTemplate
        {
            Id = 84,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 85 },
            },
        },
        new NodeTemplate
        {
            Id = 85,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 93 },
                { EdgeDirection.MiddleRight, 86 },
            },
        },
        new NodeTemplate
        {
            Id = 86,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 87 },
            },
        },
        new NodeTemplate
        {
            Id = 87,
            Category = PowerUpCategory.Health,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 88 },
            },
        },
        new NodeTemplate
        {
            Id = 88,
            Category = PowerUpCategory.Speed,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 89 },
            },
        },
        new NodeTemplate
        {
            Id = 89,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 90 },
            },
        },
        new NodeTemplate
        {
            Id = 90,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopRight, 91 },
            },
        },
        new NodeTemplate
        {
            Id = 91,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 92 },
            },
        },
        new NodeTemplate
        {
            Id = 92,
            Category = PowerUpCategory.Health,
            Rarity = NodeRarity.Legendary,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 93,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 94 },
            },
        },
        new NodeTemplate
        {
            Id = 94,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 99 },
                { EdgeDirection.BottomLeft, 95 },
            },
        },
        new NodeTemplate
        {
            Id = 95,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 96 },
            },
        },
        new NodeTemplate
        {
            Id = 96,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 97 },
            },
        },
        new NodeTemplate
        {
            Id = 97,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 98 },
            },
        },
        new NodeTemplate
        {
            Id = 98,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Legendary,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 99,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 100 },
            },
        },
        new NodeTemplate
        {
            Id = 100,
            Category = PowerUpCategory.Speed,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 101 },
            },
        },
        new NodeTemplate
        {
            Id = 101,
            Category = PowerUpCategory.DamageEffects,
            Rarity = NodeRarity.Rare,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 102,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 103 },
            },
        },
        new NodeTemplate
        {
            Id = 103,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 104 },
            },
        },
        new NodeTemplate
        {
            Id = 104,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 105 },
            },
        },
        new NodeTemplate
        {
            Id = 105,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 110 },
                { EdgeDirection.BottomLeft, 106 },
            },
        },
        new NodeTemplate
        {
            Id = 106,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomRight, 107 },
            },
        },
        new NodeTemplate
        {
            Id = 107,
            Category = PowerUpCategory.Damage,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 108 },
            },
        },
        new NodeTemplate
        {
            Id = 108,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.TopLeft, 109 },
            },
        },
        new NodeTemplate
        {
            Id = 109,
            Category = PowerUpCategory.DamageEffects,
            Rarity = NodeRarity.Rare,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 110,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleLeft, 114 },
                { EdgeDirection.TopRight, 111 },
            },
        },
        new NodeTemplate
        {
            Id = 111,
            Category = PowerUpCategory.Speed,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.MiddleRight, 112 },
            },
        },
        new NodeTemplate
        {
            Id = 112,
            Category = PowerUpCategory.Crit,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
                { EdgeDirection.BottomLeft, 113 },
            },
        },
        new NodeTemplate
        {
            Id = 113,
            Category = PowerUpCategory.WeaponUnlock,
            Rarity = NodeRarity.Legendary,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 114,
            Category = PowerUpCategory.Utility,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
        new NodeTemplate
        {
            Id = 115,
            Category = PowerUpCategory.Health,
            Rarity = NodeRarity.Common,
            Neighbours = new Dictionary<EdgeDirection,int>
            {
            },
        },
    ],
};

}