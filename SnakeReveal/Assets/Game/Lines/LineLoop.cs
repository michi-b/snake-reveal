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
        public const string ClockwiseTurnWeightFieldName = nameof(_clockwiseTurnWeight);
        public const string TurnFieldName = nameof(_turn);

        [SerializeField, HideInInspector] private int _clockwiseTurnWeight;

        [SerializeField, HideInInspector] private Turn _turn;

        private readonly Collider2D[] _findLinesBuffer = new Collider2D[2];

        protected override Color GizmosColor => new(0f, 0.5f, 1f);
        public override bool Loop => true;

        public Turn Turn => _turn;

        protected override void PostProcessLineChanges()
        {
            _clockwiseTurnWeight = this.Sum(line => line.Direction.GetTurn(line.Next!.Direction).GetClockwiseWeight());
            _turn = _clockwiseTurnWeight switch
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

            var loopSpan = new LineSpan(breakoutLine, breakInLine);
            int turnWeight = loopSpan.SumClockwiseTurnWeight(_turn);
            Debug.Log($"Turn weight along Loop: {turnWeight}");
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