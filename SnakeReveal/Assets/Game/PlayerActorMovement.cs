using System;
using Game.Enums;
using Game.Lines;
using Game.Player;
using UnityEngine;

namespace Game
{
    public class PlayerActorMovement
    {
        private readonly PlayerActor _actor;
        private readonly PlayerActorControls _controls;
        private readonly DrawingChain _drawing;
        private readonly DrawnShape _shape;

        private readonly Simulation _simulation;

        // the line the player is currently, or was last, traveling on
        private Line _shapeTravelLine;

        // whether the player in shape travel mode is traveling in the same direction as the shape turn
        private bool _isTravelingStartToEnd;
        private PlayerMovementMode _movementMode;
        private Vector2Int _shapeTravelBreakoutPosition;
        
        private Line _drawingBreakoutLine;

        public PlayerActorMovement(Simulation simulation, PlayerActor actor, DrawnShape shape, DrawingChain drawing)
        {
            _simulation = simulation;
            _actor = actor;
            _shape = shape;
            _drawing = drawing;
            _controls = PlayerActorControls.Create();
            _shapeTravelLine = null!;
            EnterShapeTravel(shape.GetLine(_actor.Position), true);
        }

        public bool ControlsEnabled
        {
            set
            {
                if (value)
                {
                    _controls.Activate();
                }
                else
                {
                    _controls.Deactivate();
                }
            }
        }

        public void Move()
        {
            for (int moveIndex = 0; moveIndex < _actor.Speed; moveIndex++)
            {
                switch (_movementMode)
                {
                    case PlayerMovementMode.ShapeTravel:
                        MovePlayerShapeTraveling();
                        break;
                    case PlayerMovementMode.Drawing:
                        MovePlayerDrawing();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // todo: apply grid position only once per frame instead (and extrapolate)
            _actor.ApplyPosition();
        }

        private void MovePlayerShapeTraveling()
        {
#if DEBUG
            Debug.Assert(_shapeTravelLine.Contains(_actor.Position));
#endif
            bool isAtLineEnd = _actor.Position == _shapeTravelLine.GetEnd(_isTravelingStartToEnd);
            
            if (_controls.TryGetRequestedDirection(out GridDirection requestedDirection)
                && _shape.TryGetBreakoutLine(requestedDirection, _shapeTravelLine, isAtLineEnd , _isTravelingStartToEnd, out Line breakoutLine))
            {
                EnterDrawingAndMove(breakoutLine, _actor.Position, requestedDirection);
                return;
            }

            // if at line end, switch to next line and adjust direction
            if (isAtLineEnd)
            {
                Line newLine = _shapeTravelLine.GetNext(_isTravelingStartToEnd);
                _shapeTravelLine = newLine;
                Debug.Assert(_shapeTravelLine != null, nameof(_shapeTravelLine) + " != null");
                GridDirection currentLineDirection = _shapeTravelLine.GetDirection(_isTravelingStartToEnd);
                _actor.Direction = currentLineDirection;
            }

            _actor.Move();
        }

        private void MovePlayerDrawing()
        {
            if (_controls.TryGetRequestedDirection(out GridDirection requestedDirection)
                && requestedDirection != _actor.Direction 
                && requestedDirection != _actor.Direction.Reverse())
            {
                _actor.Direction = requestedDirection;
            }
            
            _actor.Move();

            if (_drawing.Contains(_actor.Position))
            {
                // reset drawing
                EnterDrawingAndMove(_shapeTravelLine, _drawing.StartPosition, _drawing.StartDirection);
                return;
            }

            _drawing.Extend(_actor.Position);

            if (_shape.TryGetReconnectionLine(_actor, out Line shapeCollisionLine))
            {
                // insert drawing into shape and switch to shape travel
                InsertionResult insertionResult = _shape.Insert(_drawing, _drawingBreakoutLine, shapeCollisionLine);
                EnterShapeTravel(insertionResult.Continuation, insertionResult.IsInTurn);
            }
        }

        private void EnterShapeTravel(Line line, bool inShapeDirection)
        {
            _shapeTravelLine = line;
            _isTravelingStartToEnd = inShapeDirection;
            _actor.Direction = _shapeTravelLine.GetDirection(_isTravelingStartToEnd);
            _movementMode = PlayerMovementMode.ShapeTravel;
        }

        private void EnterDrawingAndMove(Line breakoutLine, Vector2Int breakoutPosition, GridDirection breakoutDirection)
        {
            _drawingBreakoutLine = breakoutLine;
            _actor.Position = _shapeTravelBreakoutPosition = breakoutPosition;
            _actor.Direction = breakoutDirection;

            _actor.Move();

            _drawing.Activate(_shapeTravelBreakoutPosition, _actor.Position);
            _movementMode = PlayerMovementMode.Drawing;
        }
    }
}