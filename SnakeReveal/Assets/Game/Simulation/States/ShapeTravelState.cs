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
        private bool _isInTurn = true;

        // kinda hacky, but this is used to determine if the player has never traveled, and if so, to allow travel in both directions (at game start)
        private bool _hasNeverTraveled = true;

        private DrawnShape Shape => _simulation.Shape;
        private PlayerActor Actor => _simulation.Actor;
        public int CoveredCellCount => Shape.CoveredCellCount;

        public string Name => "ShapeTravel";

        public ShapeTravelState(PlayerSimulation simulation)
        {
            _simulation = simulation;
            ClearState();
        }

        /// <inheritdoc cref="IPlayerSimulationState.Update"/>>
        public IPlayerSimulationState Update(GridDirection requestedDirection, ref SimulationUpdateResult result)
        {
            AssertActorIsOnShape();

            _hasNeverTraveled = false;

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

            Vector2Int position = Actor.Position;
            AddContinuationDirectionInTurn(position, _isInTurn, ref result);

            if (_hasNeverTraveled)
            {
                AddContinuationDirectionInTurn(position, !_isInTurn, ref result);
            }

            GridDirection breakoutDirection = Shape.GetBreakoutDirection(_currentLine);

            result = result.WithDirection(breakoutDirection);

            result = Actor.RestrictDirectionsToAvailableInBounds(result);

            return result;
        }

        private GridDirections GetContinuationDirectionsInTurn()
        {
            var result = GridDirections.None;
            AddContinuationDirectionInTurn(Actor.Position, true, ref result);
            return result;
        }

        private void AddContinuationDirectionInTurn(Vector2Int position, bool isInTurn, ref GridDirections directions)
        {
            directions = directions.WithDirection(_currentLine.GetDirection(isInTurn));
            if (position == _currentLine.GetEnd(isInTurn))
            {
                Line next = _currentLine.GetNext(isInTurn);
                directions = directions.WithDirection(next.GetDirection(isInTurn));
            }
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

        public void Resume()
        {
            AssertActorIsOnShape();

            // again kinda hacky -> on first state enter, reevaluate "isInTurn" flag with player direction
            if (_hasNeverTraveled)
            {
                _isInTurn = GetContinuationDirectionsInTurn().Contains(Actor.Direction);
            }
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
            _isInTurn = true;
        }
    }
}