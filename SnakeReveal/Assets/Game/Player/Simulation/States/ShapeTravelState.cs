using Game.Enums;
using Game.Lines;
using UnityEngine;

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
#if DEBUG
            Debug.Assert(_currentLine != null);
            Debug.Assert(_currentLine.Contains(_actor.Position));
#endif
            bool isAtLineEnd = _actor.Position == _currentLine.GetEnd(_isTravelingStartToEnd);

            if (requestedDirection != GridDirection.None
                && _shape.TryGetBreakoutLine(requestedDirection, _currentLine, isAtLineEnd, _isTravelingStartToEnd, out Line breakoutLine))
            {
                return EnterDrawingAndMove(breakoutLine, _actor.Position, requestedDirection);
            }

            // if at line end, switch to next line and adjust direction
            if (isAtLineEnd)
            {
                _currentLine = _currentLine.GetNext(_isTravelingStartToEnd);
                GridDirection currentLineDirection = _currentLine.GetDirection(_isTravelingStartToEnd);
                _actor.Direction = currentLineDirection;
            }

            _actor.Move();

            return this;
        }

        public ShapeTravelState Initialize(DrawingState drawingState)
        {
            _drawingState = drawingState;
            return Enter(_shape.GetLine(_actor.Position), true);
        }

        public ShapeTravelState Enter(Line line, bool inShapeDirection)
        {
            _currentLine = line;
            _isTravelingStartToEnd = inShapeDirection;
            _actor.Direction = _currentLine.GetDirection(_isTravelingStartToEnd);
            return this;
        }

        private DrawingState EnterDrawingAndMove(Line breakoutLine, Vector2Int actorPosition, GridDirection breakoutDirection)
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