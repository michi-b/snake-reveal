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
        private readonly PlayerSimulation _simulation;

        private Line _currentLine;

        // whether the player travels in shape turn (start to end of lines)
        private bool _isInTurn;

        private DrawnShape Shape => _simulation.Shape;
        private PlayerActor Actor => _simulation.Actor;
        public int CoveredCellCount => Shape.CoveredCellCount;

        public ShapeTravelState(PlayerSimulation simulation)
        {
            _simulation = simulation;
            ClearState();
        }

        /// <inheritdoc cref="IPlayerSimulationState.Move"/>>
        public IPlayerSimulationState Move(GridDirection requestedDirection)
        {
            AssertActorIsOnShape();

            bool isAtEndCorner = Actor.Position == _currentLine.GetEnd(_isInTurn);

            if (Actor.GetCanMoveInGridBounds(requestedDirection)
                && Shape.TryGetBreakoutLine(requestedDirection, _currentLine, isAtEndCorner, _isInTurn, out Line breakoutLine))
            {
                // note: drawing state instantly moves and might return this state again on instant reconnection
                return EnterDrawingAndMove(breakoutLine, Actor.Position, requestedDirection);
            }

            // if at line end, switch to next line and adjust direction
            if (isAtEndCorner)
            {
                _currentLine = _currentLine.GetNext(_isInTurn);
                GridDirection currentLineDirection = _currentLine.GetDirection(_isInTurn);
                Actor.Direction = currentLineDirection;
            }

            Actor.Move();
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

            GridDirection breakoutDirection = _currentLine.GetDirection(_isInTurn).Turn(Shape.GetTravelTurn(_isInTurn).Reverse());
            result = result.WithDirection(breakoutDirection);

            result = Actor.RestrictDirectionsToAvailableInBounds(result);

            return result;
        }

        public string Name => "ShapeTravel";

        private bool TryGetCornerContinuationDirection(out GridDirection direction)
        {
            if (Actor.Position != _currentLine.GetEnd(_isInTurn))
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
            if (!_currentLine.Contains(Actor.Position))
            {
                throw new InvalidOperationException("Actor is not on shape");
            }
#endif
        }

        public IPlayerSimulationState Initialize()
        {
            Line continuation = Shape.GetLine(Actor.Position);
            Debug.Assert(continuation != null, "Player actor is not on shape");
            Actor.Initialize(continuation.Direction, continuation.Previous!.Direction);
            return Enter(new InsertionResult(continuation, true));
        }

        public ShapeTravelState Enter(InsertionResult insertion)
        {
            ClearState();
            _currentLine = insertion.Continuation;
            _isInTurn = insertion.IsStartToEnd;
            Actor.Direction = _currentLine.GetDirection(_isInTurn);
            return this;
        }

        private IPlayerSimulationState EnterDrawingAndMove(Line breakoutLine, Vector2Int actorPosition, GridDirection breakoutDirection)
        {
            ClearState();
            return _simulation.DrawingState.EnterDrawingAndMove(breakoutLine, actorPosition, breakoutDirection);
        }

        private void ClearState()
        {
            _currentLine = null;
            _isInTurn = false;
        }
    }
}