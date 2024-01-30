using System;
using Game.Enums;
using Game.Grid;
using Game.Lines;
using Game.Lines.Insertion;
using Game.Player;
using Game.Quads;
using Game.Quads.Quadrangulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game
{
    public class DrawnShape : MonoBehaviour
    {
        [SerializeField] private LineLoop _lineLoop;
        [SerializeField] private QuadContainer _quadContainer;

        private readonly InsertionEvaluation _insertionEvaluation = new();
        private readonly BottomUpQuadrangulation _quadrangulation = new();
        public int CoveredCellCount => _quadContainer.CoveredCellCount;
        public SimulationGrid Grid => _lineLoop.Grid;

        private Turn GetTravelTurn(bool startToEnd)
        {
            return _lineLoop.GetTurn(startToEnd);
        }

        public Line GetLine(Vector2Int position)
        {
            return _lineLoop.TryGetFirstLineAt(position, out Line line) ? line : throw new InvalidOperationException("Actor is not on shape");
        }

        public bool TryGetReconnectionLine(PlayerActor actor, out Line line)
        {
            // lines the player can collide with must be directed like the player direction, but turned left/right opposite of the shape turn
            GridDirection collisionLinesDirection = actor.Direction.Turn(_lineLoop.Turn.Reverse());
            return _lineLoop.TryGetLineAt(actor.Position, collisionLinesDirection, out line);
        }

        public InsertionResult Insert(DrawingChain drawing, Line breakoutLine, Line reinsertionLine)
        {
            _insertionEvaluation.Evaluate(_lineLoop.Turn, drawing.Lines, breakoutLine, reinsertionLine);

            InsertionResult insertionResult = _lineLoop.Insert(_insertionEvaluation);

            _quadContainer.AddRange(_quadrangulation.Evaluate(_insertionEvaluation.Loop));

            return insertionResult;
        }

        public bool TryGetBreakoutLine(GridDirection direction, Line activeLine, bool isAtEndCorner, bool startToEnd, out Line breakoutLine)
        {
            if (isAtEndCorner)
            {
                Line next = startToEnd ? activeLine.Next! : activeLine.Previous!;
                GridDirection currentDirection = activeLine.GetDirection(startToEnd);
                GridDirection nextDirection = next.GetDirection(startToEnd);
                Turn cornerTurn = currentDirection.GetTurn(nextDirection);
                Turn travelTurn = GetTravelTurn(startToEnd);
                if (cornerTurn == travelTurn) // open corner
                {
                    if (IsBreakoutDirection(direction, activeLine))
                    {
                        breakoutLine = activeLine;
                        return true;
                    }

                    if (IsBreakoutDirection(direction, next))
                    {
                        breakoutLine = next;
                        return true;
                    }
                }

                // closed corner
                breakoutLine = null;
                return false;
            }

            if (IsBreakoutDirection(direction, activeLine))
            {
                breakoutLine = activeLine;
                return true;
            }

            breakoutLine = null;
            return false;
        }

        private bool IsBreakoutDirection(GridDirection direction, Line line)
        {
            return direction == line.Direction.Turn(_lineLoop.Turn.Reverse());
        }

#if UNITY_EDITOR
        public void EditModeRegenerateQuads()
        {
            _quadContainer.EditModeSetQuads(_quadrangulation.Evaluate(_lineLoop));
        }
#endif
    }
}