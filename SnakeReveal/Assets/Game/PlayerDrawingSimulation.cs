using System;
using Game.Enums;
using Game.Lines;
using Game.Player;
using UnityEngine;

namespace Game
{
    public class PlayerDrawingSimulation
    {
        private readonly PlayerActor _actor;
        private readonly PlayerActorControls _controls;
        private readonly DrawingChain _drawing;
        private readonly DrawnShape _shape;
        private readonly Simulation _simulation;

        // whether the player in shape travel mode is traveling in the same direction as the shape turn
        private bool _isTravelingInShapeDirection;
        private PlayerMovementMode _movementMode;
        private Vector2Int _shapeTravelBreakoutPosition;
        // the line the player is currently, or was last, traveling on
        private Line _activeShapeLine;

        public PlayerDrawingSimulation(Simulation simulation, PlayerActor actor, DrawnShape shape, DrawingChain drawing)
        {
            _simulation = simulation;
            _actor = actor;
            _shape = shape;
            _drawing = drawing;
            _controls = new PlayerActorControls(RequestDirectionChange);
            _activeShapeLine = null!;
            EnterShapeTravel(shape.GetLine(_actor.Position), true);
        }

        public bool ControlsEnabled
        {
            set
            {
                if (value)
                {
                    _controls.Enable();
                }
                else
                {
                    _controls.Disable();
                }
            }
        }

        public void Tick()
        {
            _controls.EvaluateDirectionRequests();
            
            for (int moveIndex = 0; moveIndex < _actor.Speed; moveIndex++)
            {
                switch (_movementMode)
                {
                    case PlayerMovementMode.ShapeTravel:
                        EvaluateShapeTraveling();
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

        private void RequestDirectionChange(GridDirection direction)
        {
            Turn turn = _actor.Direction.GetTurn(direction);
            if(turn == Turn.None)
            {
                return;
            }

            bool isValidTurn = _movementMode switch
            {
                PlayerMovementMode.ShapeTravel => GetIsValidShapeTravelTurn(turn),
                PlayerMovementMode.Drawing => true,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (isValidTurn)
            {
                _actor.Direction = direction;
            }
        }

        private bool GetIsValidShapeTravelTurn(Turn turn)
        {
            bool isBreakoutTurn = _shape.GetTurn(_isTravelingInShapeDirection) switch
            {
                Turn.None => throw new ArgumentOutOfRangeException(),
                Turn.Right => turn == Turn.Left,
                Turn.Left => turn == Turn.Right,
                _ => throw new ArgumentOutOfRangeException()
            };
            return isBreakoutTurn;
        }

        private void EvaluateShapeTraveling()
        {
#if DEBUG
            Debug.Assert(_activeShapeLine.Contains(_actor.Position));
#endif

            if (_actor.Direction == _activeShapeLine.GetDirection(_isTravelingInShapeDirection))
            {
                // evaluate whether current travel line and consequently the current player direction need to be adjusted
                ContinueShapeTravel();
            }
            else
            {
                EnterDrawing(_activeShapeLine, _actor.Position);
            }
        }

        private void ContinueShapeTravel()
        {
            bool isAtLineEnd = _actor.Position.Equals(_activeShapeLine.GetEnd(_isTravelingInShapeDirection));

            // if at line end, switch to next line and adjust direction
            if (isAtLineEnd)
            {
                Line newLine = _activeShapeLine.GetNext(_isTravelingInShapeDirection);
                _activeShapeLine = newLine;
                Debug.Assert(_activeShapeLine != null, nameof(_activeShapeLine) + " != null");
                GridDirection currentLineDirection = _activeShapeLine.GetDirection(_isTravelingInShapeDirection);
                _actor.Direction = currentLineDirection;
            }

            _actor.Step();
        }

        private void MovePlayerDrawing()
        {
            _actor.Step();

            if (_drawing.Contains(_actor.Position))
            {
                // reset drawing
                EnterDrawing(_activeShapeLine, _drawing.StartPosition);
                return;
            }

            _drawing.Extend(_actor.Position);

            if (_shape.TryGetReconnectionLine(_actor, out Line shapeCollisionLine))
            {
                // insert drawing into shape and switch to shape travel
                InsertionResult insertionResult = _shape.Insert(_drawing, _activeShapeLine, shapeCollisionLine);
                EnterShapeTravel(insertionResult.Continuation, insertionResult.IsInTurn);
            }
        }

        private void EnterShapeTravel(Line line, bool inShapeDirection)
        {
            _activeShapeLine = line;
            _isTravelingInShapeDirection = inShapeDirection;
            _actor.Direction = _activeShapeLine.GetDirection(_isTravelingInShapeDirection);
            _movementMode = PlayerMovementMode.ShapeTravel;
        }

        private void EnterDrawing(Line breakoutLine, Vector2Int breakoutPosition)
        {
            _shapeTravelBreakoutPosition = breakoutPosition;
            _actor.Direction = breakoutLine.Direction.Turn(_shape.Turn.Reverse());
            _actor.Position = breakoutPosition;
            
            _actor.Step();

            _drawing.Activate(_shapeTravelBreakoutPosition, _actor.Position);
            _movementMode = PlayerMovementMode.Drawing;
        }
    }
}