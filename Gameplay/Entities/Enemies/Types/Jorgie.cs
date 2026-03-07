using System;
using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.SpriteSheets;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Enemies.Types;

public class Jorgie : EnemyBase, IGenericVisual
{
    private const float SegmentSpacing = 64f; // tune vs sprite size
    private const int SegmentCount = 24;
    private const int HistoryLimit = 2048;

    private readonly static EnemyStats StatValues = new(4000f, 100f, 2, 0.05f);
    private readonly Action<EnemyBase> _customOnDeath;

    private readonly SnakeBossHeadSheet _headSpriteSheet;
    private readonly Vector2 _headOrigin;
    private readonly Texture2D _bodyTexture;
    private readonly Vector2 _bodyOrigin;
    private readonly Texture2D _tailTexture;
    private readonly Vector2 _tailOrigin;

    private readonly List<SnakeSegment> _segments = [];
    private readonly List<Vector2> _positionHistory = [];


    public Jorgie(SpawnContext spawnContext, Action<EnemyBase> onDeath) :
        base(spawnContext, StatValues, GetMovement(spawnContext.Player), ColorPalette.Green, false)
    {
        _customOnDeath = onDeath;
        _headSpriteSheet = new SnakeBossHeadSheet(spawnContext.Content);
        _bodyTexture = spawnContext.Content.Load<Texture2D>(Paths.Images.SnakeBody);
        _tailTexture = spawnContext.Content.Load<Texture2D>(Paths.Images.SnakeTail);

        _headOrigin = _headSpriteSheet.FrameSize * 0.5f;
        _bodyOrigin = new Vector2(_bodyTexture.Width * 0.5f, _bodyTexture.Height * 0.5f);
        _tailOrigin = new Vector2(_tailTexture.Width * 0.5f, _tailTexture.Height * 0.5f);

        // Initialise history so the snake doesn't collapse on spawn
        for (var i = 0; i < HistoryLimit; i++) _positionHistory.Add(Position);

        // Create body segments
        for (var i = 0; i < SegmentCount; i++) _segments.Add(new SnakeSegment(Position));

        var headCollider = new CircleCollider(this, 96f);
        Colliders = [headCollider, .._segments.Select(s => s.Collider)];
        Behaviours =
        [
            new SnakeSegmentsBehaviour(this, _positionHistory, HistoryLimit),
        ];
    }

    public override bool AffectedBySeparationForces => false;

    public void Draw(SpriteBatch spriteBatch)
    {
        var layer = Layers.Enemies + 0.02f;

        // Head
        var headFrame = new SnakeBossHeadSheet.HeadDirectionFrame(Velocity);
        var headRectangle = _headSpriteSheet.GetFrameRectangle(headFrame);
        spriteBatch.Draw(_headSpriteSheet.Texture, Position, ColorPalette.White, headRectangle, origin: _headOrigin,
            layerDepth: layer, scale: DrawScale);

        // Body
        layer -= 0.00001f;
        foreach (var segment in _segments[..^1]) // skip last
        {
            spriteBatch.Draw(_bodyTexture, segment.Position, origin: _bodyOrigin, layerDepth: layer, scale: DrawScale,
                rotation: segment.Rotation);
            layer -= 0.00001f;
        }

        // Tail
        var tail = _segments[^1];
        spriteBatch.Draw(_tailTexture, tail.Position, origin: _tailOrigin, layerDepth: layer, scale: DrawScale,
            rotation: tail.Rotation);
    }

    protected override void OnDeath(EnemyBase enemy)
    {
        base.OnDeath(enemy);
        _customOnDeath(enemy);
    }

    private static SlitherFollowEntity GetMovement(PlayerCharacter player) => new(player, 0.12f, 2f, 0.05f);

    private void UpdateSegments()
    {
        var distanceAccumulated = 0f;
        var historyIndex = 0;

        for (var i = 0; i < _segments.Count; i++)
        {
            var targetDistance = (i + 1) * SegmentSpacing;

            // Walk the history until we reach the spacing we want
            while (historyIndex + 1 < _positionHistory.Count)
            {
                var step = Vector2.Distance(
                    _positionHistory[historyIndex],
                    _positionHistory[historyIndex + 1]);

                if (distanceAccumulated + step >= targetDistance)
                    break;

                distanceAccumulated += step;
                historyIndex++;
            }

            var segment = _segments[i];
            segment.Position = _positionHistory[historyIndex];

            var targetAhead = i == 0
                ? Position
                : _segments[i - 1].Position;

            var direction = targetAhead - segment.Position;
            if (direction.LengthSquared() > 0.0001f)
            {
                var angle = MathF.Atan2(direction.Y, direction.X) + MathF.PI * 0.5f;

                const float step = MathF.Tau / 32f;
                segment.Rotation = MathF.Round(angle / step) * step;
            }
        }
    }

    private sealed class SnakeSegment : IHasPosition
    {
        public SnakeSegment(Vector2 position)
        {
            Position = position;
            Collider = new CircleCollider(this, 64f);
        }

        public CircleCollider Collider { get; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
    }

    private sealed class SnakeSegmentsBehaviour(Jorgie boss, List<Vector2> history, int historyLimit)
        : IEnemyBehaviour
    {
        public void BeforeMove(GameTime gameTime) { }

        public void AfterMove(GameTime gameTime)
        {
            // Record head position after movement
            history.Insert(0, boss.Position);

            if (history.Count > historyLimit)
                history.RemoveAt(history.Count - 1);

            boss.UpdateSegments();
        }
    }
}