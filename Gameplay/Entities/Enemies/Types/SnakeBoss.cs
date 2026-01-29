using System;
using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.SpriteSheets;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Enemies.Types;

public class SnakeBoss : EnemyBase, IGenericVisual
{
    private const float SegmentSpacing = 64f; // tune vs sprite size
    private const int SegmentCount = 12;
    private const int HistoryLimit = 2048;

    private readonly static EnemyStats StatValues = new(4000f, 100f, 2, 0.05f);
    private readonly Action<EnemyBase> _customOnDeath;
    private readonly FollowEntity _followEntity;

    private readonly SnakeBossHeadSheet _headSpriteSheet;
    private readonly Vector2 _headOrigin;
    private readonly Texture2D _bodyTexture;
    private readonly Vector2 _bodyOrigin;

    private readonly List<SnakeSegment> _segments = [];
    private readonly List<Vector2> _positionHistory = [];


    public SnakeBoss(ContentManager content, Vector2 initialPosition, IHasPosition target, Action<EnemyBase> onDeath,
        EnemyDeathHandler deathHandler) :
        base(initialPosition, StatValues, deathHandler)
    {
        _customOnDeath = onDeath;
        _followEntity = new FollowEntity(this, target, 0.08f);
        _headSpriteSheet = new SnakeBossHeadSheet(content);
        _bodyTexture = content.Load<Texture2D>(Paths.Images.SnakeBody);

        _headOrigin = _headSpriteSheet.FrameSize * 0.5f;
        _bodyOrigin = new Vector2(_bodyTexture.Width * 0.5f, _bodyTexture.Height * 0.5f);

        // Initialise history so the snake doesn't collapse on spawn
        for (var i = 0; i < HistoryLimit; i++) _positionHistory.Add(Position);

        // Create body segments
        for (var i = 0; i < SegmentCount; i++) _segments.Add(new SnakeSegment(Position));

        var headCollider = new CircleCollider(this, 96f);
        Colliders = [headCollider, .._segments.Select(s => s.Collider)];
    }

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
        foreach (var segment in _segments)
        {
            spriteBatch.Draw(_bodyTexture, segment.Position, ColorPalette.White, origin: _bodyOrigin,
                layerDepth: layer, scale: DrawScale);
            layer -= 0.00001f;
        }
    }

    protected override void OnDeath(EnemyBase enemy)
    {
        base.OnDeath(enemy);
        _customOnDeath(enemy);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        IntentVelocity = _followEntity.CalculateVelocity(SeparationForce);

        // Record head position
        _positionHistory.Insert(0, Position);

        if (_positionHistory.Count > HistoryLimit)
            _positionHistory.RemoveAt(_positionHistory.Count - 1);

        UpdateSegments();
    }

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

            _segments[i].Position = _positionHistory[historyIndex];
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
    }
}