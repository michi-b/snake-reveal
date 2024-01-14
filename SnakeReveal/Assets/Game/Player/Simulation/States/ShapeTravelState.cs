using System;
using System.Diagnostics;
using Game.Enums;
using Game.Lines;
using Game.Lines.Insertion;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Player.Simulation.States
{
    public class ShapeTravelState : IPlayerSimulationState
    {
        private readonly PlayerActor _actor;
        private readonly DrawnShape _shape;

        private Line _currentLine;
        private DrawingState _drawingState;
        private bool _isTravelingStartToEnd;

        public ShapeTravelState(PlayerActor actor, DrawnShape shape)
        {
            _actor = actor;
            _shape = shape;
            ClearState();
        }

        public IPlayerSimulationState Move(GridDirection requestedDirection)
        {
            AssertActorIsOnShape();

            bool isAtEndCorner = _actor.Position == _currentLine.GetEnd(_isTravelingStartToEnd);

            if (requestedDirection != GridDirection.None
                && _shape.TryGetBreakoutLine(requestedDirection, _currentLine, isAtEndCorner, _isTravelingStartToEnd, out Line breakoutLine))
            {
                // note: drawing state instantly moves and might return this state again on instant reconnection
                return EnterDrawingAndMove(breakoutLine, _actor.Position, requestedDirection);
            }

            // if at line end, switch to next line and adjust direction
            if (isAtEndCorner)
            {
                _currentLine = _currentLine.GetNext(_isTravelingStartToEnd);
                GridDirection currentLineDirection = _currentLine.GetDirection(_isTravelingStartToEnd);
                _actor.Direction = currentLineDirection;
            }

            _actor.Move();
            AssertActorIsOnShape();
            return this;
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
            return Enter(new InsertionResult(continuation, true));
        }

        public ShapeTravelState Enter(InsertionResult insertion)
        {
            _currentLine = insertion.Continuation;
            _isTravelingStartToEnd = insertion.IsStartToEnd;
            _actor.Direction = _currentLine.GetDirection(_isTravelingStartToEnd);
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
            _isTravelingStartToEnd = false;
        }
    }
}