using System;
using System.Diagnostics;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Lines;
using Game.Lines.Insertion;
using Game.Player;
using Debug = UnityEngine.Debug;

namespace Game.Simulation.States
{
    public class ShapeTravelState : IPlayerSimulationState
    {
        private readonly PlayerSimulation _simulation;

        private Line _currentLine;

        // whether the player travels in shape turn (start to end of lines)
        private bool _isInTurn = true;

        private bool _isJustInitialized = true;

        private DrawnShape Shape => _simulation.Shape;
        private PlayerActor Actor => _simulation.Actor;
        public int CoveredCellCount => Shape.CoveredCellCount;

        public ShapeTravelState(PlayerSimulation simulation)
        {
            _simulation = simulation;
            ClearState();
        }

        /// <inheritdoc cref="IPlayerSimulationState.Update"/>>
        public IPlayerSimulationState Update(GridDirection requestedDirection, ref SimulationUpdateResult result)
        {
            _isJustInitialized = false;

            AssertActorIsOnShape();

            bool isAtEndCorner = Actor.Position == _currentLine.GetEnd(_isInTurn);

            if (Actor.GetCanMoveInGridBounds(requestedDirection)
                && Shape.TryGetBreakoutLine(requestedDirection, _currentLine, isAtEndCorner, _isInTurn, out Line breakoutLine))
            {
                // note: drawing state instantly moves and might return to this state again on collision or instant reconnection
                return _simulation.DrawingState.EnterDrawingAndMove(breakoutLine, Actor.Position, requestedDirection, ref result);
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

            if (_isJustInitialized)
            {
                result = result.WithDirection(_currentLine.GetDirection(!_isInTurn));
            }

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
            _currentLine = Shape.GetLine(Actor.Position);
            Debug.Assert(_currentLine != null, "Player actor is not on shape");
            Actor.Initialize(_currentLine.Direction, _currentLine.Previous!.Direction);
            return ReEnter();
        }

        public ShapeTravelState ReEnter()
        {
            Actor.Direction = _currentLine.GetDirection(_isInTurn);
            return this;
        }

        public ShapeTravelState Enter(InsertionResult insertion, ref SimulationUpdateResult result)
        {
            ClearState();
            _currentLine = insertion.Continuation;
            _isInTurn = insertion.IsStartToEnd;
            Actor.Direction = _currentLine.GetDirection(_isInTurn);
            return this;
        }

        private void ClearState()
        {
            _currentLine = null;
            _isInTurn = false;
        }
    }
}