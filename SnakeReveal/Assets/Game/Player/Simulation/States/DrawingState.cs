using Game.Enums;
using Game.Lines;
using Game.Lines.Insertion;
using UnityEngine;

namespace Game.Player.Simulation.States
{
    public class DrawingState : IPlayerSimulationState
    {
        private readonly PlayerActor _actor;
        private readonly DrawingChain _drawing;
        private readonly DrawnShape _shape;
        private readonly ShapeTravelState _shapeTravelState;
        private Line _shapeBreakoutLine;
        private Vector2Int _shapeBreakoutPosition;

        public DrawingState(PlayerActor actor, DrawnShape shape, DrawingChain drawing, ShapeTravelState shapeTravelState)
        {
            _actor = actor;
            _shape = shape;
            _drawing = drawing;
            _shapeTravelState = shapeTravelState;
            ClearState();
        }

        public IPlayerSimulationState Move(GridDirection requestedDirection)
        {
            if (requestedDirection != GridDirection.None
                && requestedDirection != _actor.Direction
                && requestedDirection != _actor.Direction.Reverse())
            {
                _actor.Direction = requestedDirection;
            }

            return Move();
        }

        public IPlayerSimulationState EnterDrawingAndMove(Line shapeBreakoutLine, Vector2Int breakoutPosition, GridDirection breakoutDirection)
        {
            _shapeBreakoutLine = shapeBreakoutLine;
            _actor.Position = _shapeBreakoutPosition = breakoutPosition;
            _actor.Direction = breakoutDirection;

            _drawing.Activate(_shapeBreakoutPosition, _actor.Position);

            _actor.Move();

#if DEBUG
            Debug.Assert(!_drawing.Contains(_actor.Position));
#endif

            _drawing.Extend(_actor.Position);

            return TryReconnect();
        }

        private IPlayerSimulationState Move()
        {
            _actor.Move();

            if (_drawing.Contains(_actor.Position))
            {
                return EnterDrawingAndMove(_shapeBreakoutLine, _drawing.StartPosition, _drawing.StartDirection);
            }

            _drawing.Extend(_actor.Position);

            return TryReconnect();
        }

        private IPlayerSimulationState TryReconnect()
        {
            if (_shape.TryGetReconnectionLine(_actor, out Line shapeCollisionLine))
            {
                // insert drawing into shape and switch to shape travel
                InsertionResult insertionResult = _shape.Insert(_drawing, _shapeBreakoutLine, shapeCollisionLine);
                ClearState();
                return _shapeTravelState.Enter(insertionResult);
            }

            return this;
        }

        private void ClearState()
        {
            _drawing.Deactivate();
            _shapeBreakoutLine = null;
            _shapeBreakoutPosition = new Vector2Int(-1, -1);
        }
    }
}