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

        // kinda hacky, but this is used to determine if the player has never traveled, and if so, to allow travel in both directions (at game start)
        private bool _hasNeverMovedOnShape = true;

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
        public IPlayerSimulationState Update(ref SimulationUpdateResult result)
        {
            AssertActorIsOnShape();


            if (Shape.TryGetBreakoutLine(Actor.Position, Actor.Direction, _currentLine, out Line breakoutLine))
            {
                // note: drawing state instantly moves and might return to this state again on collision or instant reconnection
                return _simulation.DrawingState.EnterDrawingAndMove(breakoutLine, ref result);
            }

            Actor.Move();
            _hasNeverMovedOnShape = false;

            EvaluateActorAtLineEnd();

            AssertActorIsOnShape();

            return this;
        }

        /// <inheritdoc cref="IPlayerSimulationState.GetAvailableDirections"/>>
        public GridDirections GetAvailableDirections()
        {
#if DEBUG
            Debug.Assert(!IsAtLineEnd);
#endif

            var result = GridDirections.None;

            GridDirection continuationDirection = GetContinuationDirection();
            GridDirection counterContinuationDirection = GetContinuationDirection(!_isInTurn);

            result = result.WithDirection(continuationDirection);

            if (_hasNeverMovedOnShape)
            {
                // only if the player has never moved, he can choose a direction on the starting line
                result = result.WithDirection(counterContinuationDirection);
            }

            result = result.WithDirectionsBetween(counterContinuationDirection, continuationDirection, Shape.GetTravelTurn(_isInTurn));

            result = Actor.RestrictDirectionsInBounds(result);

            return result;
        }

        private GridDirection GetContinuationDirection() => GetContinuationDirection(_isInTurn);

        private GridDirection GetContinuationDirection(bool inTurn) =>
            Actor.Position == _currentLine.GetEnd(inTurn) ? _currentLine.GetNext(inTurn).GetDirection(inTurn) : _currentLine.GetDirection(inTurn);

        private GridDirections GetContinuationDirectionsInTurn()
        {
            var result = GridDirections.None;
            result = result.WithDirection(_currentLine.GetDirection());
            return result;
        }

        /// <summary>
        /// If at line end, switch to next line and adjust direction
        /// </summary>
        private void EvaluateActorAtLineEnd()
        {
            if (IsAtLineEnd)
            {
                _currentLine = _currentLine.GetNext(_isInTurn);
                GridDirection currentLineDirection = _currentLine.GetDirection(_isInTurn);
                Actor.Direction = currentLineDirection;
            }
        }

        private bool IsAtLineEnd => Actor.Position == _currentLine.GetEnd(_isInTurn);

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
            if (_hasNeverMovedOnShape)
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
            _isInTurn = insertion.IsStartToEnd;
            _currentLine = insertion.Continuation;
            EvaluateActorAtLineEnd();
            return this;
        }

        private void ClearState()
        {
            _currentLine = null;
            _isInTurn = true;
        }
    }
}