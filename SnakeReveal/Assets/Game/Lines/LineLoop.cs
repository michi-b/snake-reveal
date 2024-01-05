using System;
using System.Linq;
using Extensions;
using Game.Enums;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Lines
{
    public class LineLoop : LineContainer
    {
        public const string TurnFieldName = nameof(_turn);


        [SerializeField, HideInInspector] private Turn _turn;

        private readonly Collider2D[] _findLinesBuffer = new Collider2D[2];

        protected override Color GizmosColor => new(0f, 0.5f, 1f);
        public override bool Loop => true;
        public Turn Turn => _turn;

        protected override void PostProcessLineChanges()
        {
            base.PostProcessLineChanges();

            _turn = ClockwiseTurnWeight switch
            {
                4 => Turn.Right,
                -4 => Turn.Left,
                _ => Turn.None
            };
        }

        public void EditModeInsert(LineChain insertTarget)
        {
            Line chainStart = insertTarget.Start;
            GridDirection breakoutDirection = chainStart.Direction.Turn(_turn);
            Line breakoutLine = FindLine(chainStart.Start, breakoutDirection);
            Debug.Assert(breakoutLine != null, "No suitable breakout line on loop.");

            Line chainEnd = insertTarget.Last();
            GridDirection breakInDirection = chainEnd.Direction.Turn(_turn.Reverse());
            Line breakInLine = FindLine(chainEnd.End, breakInDirection);
            Debug.Assert(breakInLine != null, "No suitable break-in on loop.");

            int loopTurnClockwiseWeight = new LineSpan(breakoutLine, breakInLine).SumClockwiseTurnWeight();
            int chainTurnClockwiseWeight = insertTarget.AsSpan(true).SumClockwiseTurnWeight();
            int deltaClockwiseWeight = chainTurnClockwiseWeight - loopTurnClockwiseWeight;
            bool reconnectsInTurn = Turn switch
            {
                Turn.None => throw new ArgumentOutOfRangeException(),
                Turn.Right => deltaClockwiseWeight > 0,
                Turn.Left => deltaClockwiseWeight < 0,
                _ => throw new ArgumentOutOfRangeException()
            };
            Debug.Log(reconnectsInTurn ? "Chain reconnects IN turn" : "Chain reconnects COUNTER turn");
        }

        [CanBeNull]
        private Line FindLine(Vector2Int position, GridDirection direction)
        {
            foreach (Collider2D lineCollider in FindColliders(position))
            {
                if (lineCollider.TryGetComponent(out Line line))
                {
                    if (line.Direction == direction)
                    {
                        return line;
                    }
                }
            }

            return null;
        }

        private Span<Collider2D> FindColliders(Vector2Int position)
        {
            var contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = LayerMask
            };

            int count = Physics2D.OverlapPoint(position.GetScenePosition(Grid), contactFilter, _findLinesBuffer);

#if DEBUG
            Debug.Assert(count < _findLinesBuffer.Length, $"Overlap point count {count} is larger than collider buffer size {_findLinesBuffer.Length}, " +
                                                          " which should not be possible by design (usage of layers and avoiding overlapping lines).");
#endif

            return new Span<Collider2D>(_findLinesBuffer, 0, count);
        }
    }
}