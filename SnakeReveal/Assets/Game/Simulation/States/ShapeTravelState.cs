using System;
using System.Diagnostics;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Lines;
using Game.Lines.Insertion;
using Game.Player;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Simulation.States
{
    public class ShapeTravelState : IPlayerSimulationState
    {
        private readonly PlayerActor _actor;
        private readonly DrawnShape _shape;

        private Line _currentLine;
        private DrawingState _drawingState;

        // whether the player travels in shape turn (start to end of lines)
        private bool _isInTurn;

        public ShapeTravelState(PlayerActor actor, DrawnShape shape)
        {
            _actor = actor;
            _shape = shape;
            ClearState();
        }

        public int CoveredCellCount => _shape.CoveredCellCount;

        /// <inheritdoc cref="IPlayerSimulationState.Move"/>>
        public IPlayerSimulationState Move(GridDirection requestedDirection)
        {
            AssertActorIsOnShape();

            bool isAtEndCorner = _actor.Position == _currentLine.GetEnd(_isInTurn);

            if (_actor.GetCanMoveInGridBounds(requestedDirection)
                && _shape.TryGetBreakoutLine(requestedDirection, _currentLine, isAtEndCorner, _isInTurn, out Line breakoutLine))
            {
                // note: drawing state instantly moves and might return this state again on instant reconnection
                return EnterDrawingAndMove(breakoutLine, _actor.Position, requestedDirection);
            }

            // if at line end, switch to next line and adjust direction
            if (isAtEndCorner)
            {
                _currentLine = _currentLine.GetNext(_isInTurn);
                GridDirection currentLineDirection = _currentLine.GetDirection(_isInTurn);
                _actor.Direction = currentLineDirection;
            }

            _actor.Move();
            AssertActorIsOnShape();
            return this;
        }

        /// <inheritdoc cref="IPlayerSimulationState.GetAvailableDirections"/>>
        public GridDirections GetAvailableDirections()
        {
            var result = GridDirections.None;
            result = result.WithDirection(_currentLine.GetDirection(_isInTurn));

            if (TryGetCornerContinuationDirection(out GridDirection cornerContinuationDirection2))
            {
                result = result.WithDirection(cornerContinuationDirection2);
            }

            GridDirection breakoutDirection = _currentLine.GetDirection(_isInTurn).Turn(_shape.GetTravelTurn(_isInTurn).Reverse());
            result = result.WithDirection(breakoutDirection);

            result = _actor.RestrictDirectionsToAvailableInBounds(result);

            return result;
        }

        public string Name => "ShapeTravel";

        private bool TryGetCornerContinuationDirection(out GridDirection direction)
        {
            if (_actor.Position != _currentLine.GetEnd(_isInTurn))
            {
                direction = GridDirection.None;
                return false;
            }

            Line next = _currentLine.GetNext(_isInTurn);
            direction = next.GetDirection(_isInTurn);
            return true;
        }

        [Conditional("DEBUG")]
        private void AssertActorIsOnShape()
        {
#if DEBUG
            if (!_currentLine.Contains(_actor.Position))
            {
                throw new InvalidOperationException("Actor is not on shape");
            }
#endif
        }

        public ShapeTravelState Initialize(DrawingState drawingState)
        {
            _drawingState = drawingState;
            Line continuation = _shape.GetLine(_actor.Position);
            Debug.Assert(continuation != null, "Player actor is not on shape");
            _actor.Initialize(continuation.Direction, continuation.Previous!.Direction);
            return Enter(new InsertionResult(continuation, true));
        }

        public ShapeTravelState Enter(InsertionResult insertion)
        {
            ClearState();
            _currentLine = insertion.Continuation;
            _isInTurn = insertion.IsStartToEnd;
            _actor.Direction = _currentLine.GetDirection(_isInTurn);
            return this;
        }

        private IPlayerSimulationState EnterDrawingAndMove(Line breakoutLine, Vector2Int actorPosition, GridDirection breakoutDirection)
        {
            ClearState();
            return _drawingState.EnterDrawingAndMove(breakoutLine, actorPosition, breakoutDirection);
        }

        private void ClearState()
        {
            _currentLine = null;
            _isInTurn = false;
        }
    }
}