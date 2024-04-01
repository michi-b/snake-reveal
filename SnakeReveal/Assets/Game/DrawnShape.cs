using System;
using System.Linq;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Lines;
using Game.Lines.Insertion;
using Game.Player;
using Game.Quads;
using Game.Quads.Quadrangulation;
using Game.Simulation.Grid;
using UnityEngine;

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

        public Turn GetTravelTurn(bool startToEnd) => _lineLoop.GetTurn(startToEnd);

        public Line GetLine(Vector2Int position) => _lineLoop.TryGetFirstLineAt(position, out Line line) ? line : throw new InvalidOperationException("Actor is not on shape");

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

#if DEBUG
            LineData[] insertionLoop = _insertionEvaluation.Loop.ToArray();
            Debug.Assert(insertionLoop[^1].End == insertionLoop[0].Start, "Insertion loop is not closed");
#endif

            _quadContainer.AddRange(_quadrangulation.Evaluate(_insertionEvaluation.Loop));

            return insertionResult;
        }

        public bool TryGetBreakoutLine(Vector2Int position, GridDirection direction, Line line, out Line breakoutLine)
        {
#if DEBUG
            Debug.Assert(line.Contains(position));
#endif

            if (position == line.Start)
            {
                if (TryGetBreakoutLineAtCorner(direction, line, false, out breakoutLine))
                {
                    return true;
                }
            }
            else if (position == line.End)
            {
                if (TryGetBreakoutLineAtCorner(direction, line, true, out breakoutLine))
                {
                    return true;
                }
            }
            else
            {
                if (GetBreakoutDirection(line) == direction)
                {
                    breakoutLine = line;
                    return true;
                }
            }

            breakoutLine = null;
            return false;
        }

        private bool TryGetBreakoutLineAtCorner(GridDirection direction, Line line, bool atEnd, out Line breakoutLine)
        {
            Line lineBeforeCorner;
            Line lineAfterCorner;
            if (atEnd)
            {
                lineBeforeCorner = line;
                lineAfterCorner = line.Next!;
            }
            else
            {
                lineBeforeCorner = line.Previous!;
                lineAfterCorner = line;
            }

            if (lineBeforeCorner.GetTurn(lineAfterCorner) != _lineLoop.Turn)
            {
                // closed corner
                breakoutLine = null;
                return false;
            }

            // open corner
            if (direction == GetBreakoutDirection(lineBeforeCorner))
            {
                breakoutLine = lineBeforeCorner;
                return true;
            }

            if (direction == GetBreakoutDirection(lineAfterCorner))
            {
                breakoutLine = lineAfterCorner;
                return true;
            }

            breakoutLine = null;
            return false;
        }

        private GridDirection GetBreakoutDirection(Line line) => line.Direction.Turn(_lineLoop.Turn.Reverse());

#if UNITY_EDITOR
        public void EditModeRegenerateQuads()
        {
            _quadContainer.EditModeSetQuads(_quadrangulation.Evaluate(_lineLoop));
        }
#endif
    }
}