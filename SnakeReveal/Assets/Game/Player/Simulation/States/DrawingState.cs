using Game.Enums;
using Game.Lines;
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

            _actor.Move();

            if (_drawing.Contains(_actor.Position))
            {
                return EnterDrawingAndMove(_shapeBreakoutLine, _drawing.StartPosition, _drawing.StartDirection);
            }

            _drawing.Extend(_actor.Position);

            if (_shape.TryGetReconnectionLine(_actor, out Line shapeCollisionLine))
            {
                // insert drawing into shape and switch to shape travel
                InsertionResult insertionResult = _shape.Insert(_drawing, _shapeBreakoutLine, shapeCollisionLine);
                return EnterShapeTravel(insertionResult.Continuation, insertionResult.IsInTurn);
            }

            return this;
        }

        public DrawingState EnterDrawingAndMove(Line shapeBreakoutLine, Vector2Int breakoutPosition, GridDirection breakoutDirection)
        {
            _shapeBreakoutLine = shapeBreakoutLine;
            _actor.Position = _shapeBreakoutPosition = breakoutPosition;
            _actor.Direction = breakoutDirection;

            _actor.Move();

            _drawing.Activate(_shapeBreakoutPosition, _actor.Position);
            return this;
        }

        private ShapeTravelState EnterShapeTravel(Line insertionResultContinuation, bool insertionResultIsInTurn)
        {
            ClearState();
            return _shapeTravelState.Enter(insertionResultContinuation, insertionResultIsInTurn);
        }

        private void ClearState()
        {
            _shapeBreakoutLine = null;
            _shapeBreakoutPosition = new Vector2Int(-1, -1);
        }
    }
}