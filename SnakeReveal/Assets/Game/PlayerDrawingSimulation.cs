using System;
using Game.Enums;
using Game.Lines.Deprecated;
using Game.Player;
using JetBrains.Annotations;
using UnityEngine;

namespace Game
{
    public class PlayerDrawingSimulation
    {
        private readonly PlayerActor _actor;
        private readonly PlayerActorControls _controls;

        private readonly DeprecatedLineChain _drawingLineChain;

        private readonly DeprecatedLineLoop _shape;
        private readonly Simulation _simulation;

        // whether the player in shape travel mode is traveling in the same direction as the shape turn
        private bool _isTravelingInShapeDirection;

        private int _lastDirectionChangeRequestTick = -1;

        private PlayerMovementMode _movementMode;

        private Vector2Int _shapeTravelBreakoutPosition;
        [NotNull] private DeprecatedLine _shapeTravelLine;


        public PlayerDrawingSimulation(Simulation simulation, PlayerActor actor, DeprecatedLineLoop shape, DeprecatedLineChain drawingLineChain)
        {
            _simulation = simulation;
            _actor = actor;
            _shape = shape;
            _drawingLineChain = drawingLineChain;
            _controls = new PlayerActorControls(RequestDirectionChange);
            _shapeTravelLine = _shape.FindLineAt(actor.Position, line => line.Direction.IsSameOrOpposite(actor.Direction));
            if (_shapeTravelLine == null)
            {
                throw new InvalidOperationException("Actor is not on shape");
            }

            _isTravelingInShapeDirection = _actor.Direction == _shapeTravelLine.Direction;
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

        private Turn TravelTurn => _isTravelingInShapeDirection ? _shape.Turn : _shape.Turn.GetOpposite();

        public void Tick()
        {
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

            // todo: apply grid position only once per frame instead (and extrapolated)
            _actor.ApplyPosition();
        }

        private void RequestDirectionChange(GridDirection direction)
        {
            // prevent multiple direction change requests per tick
            // which is currently the solution to avoid the player u-turning into its own current drawing line
            int currentTick = _simulation.Ticks;
            try
            {
                if (currentTick == _lastDirectionChangeRequestTick)
                {
                    return;
                }
            }
            finally
            {
                _lastDirectionChangeRequestTick = currentTick;
            }

            Turn turn = _actor.Direction.GetTurn(direction);
            if (turn == Turn.None)
            {
                return;
            }

            if (_movementMode switch
                {
                    PlayerMovementMode.ShapeTravel => GetIsValidTurnWhileShapeTraveling(turn),
                    PlayerMovementMode.Drawing => true,
                    _ => throw new ArgumentOutOfRangeException()
                })
            {
                _actor.Direction = direction;
            }
        }

        private bool GetIsValidTurnWhileShapeTraveling(Turn turn)
        {
            return TravelTurn switch
            {
                Turn.None => throw GetHasNoShapeTravelTurnException(),
                Turn.Right => turn == Turn.Left,
                Turn.Left => turn == Turn.Right,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static Exception GetHasNoShapeTravelTurnException()
        {
            return new InvalidOperationException($"Shape travel turn is {nameof(Turn.None)} although it should never have that value by design");
        }

        private void EvaluateShapeTraveling()
        {
            Debug.Assert(_shapeTravelLine.Contains(_actor.Position));

            if (_actor.Direction == _shapeTravelLine.GetDirection(_isTravelingInShapeDirection))
            {
                // evaluate whether current travel line and consequently the current player direction need to be adjusted
                ContinueShapeTraveling();
            }
            else
            {
                Breakout(_actor.Position);
            }
        }

        private void ContinueShapeTraveling()
        {
            bool isAtLineEnd = _actor.Position.Equals(_shapeTravelLine.GetEnd(_isTravelingInShapeDirection));

            // if at line end, switch to next line and adjust direction
            if (isAtLineEnd)
            {
                DeprecatedLine newLine = _shapeTravelLine.GetNext(_isTravelingInShapeDirection);
                if (newLine == null)
                {
                    throw new InvalidOperationException("Line loop is not closed");
                }

                _shapeTravelLine = newLine;
                Debug.Assert(_shapeTravelLine != null, nameof(_shapeTravelLine) + " != null");
                GridDirection currentLineDirection = _shapeTravelLine.GetDirection(_isTravelingInShapeDirection);
                _actor.Direction = currentLineDirection;
            }

            _actor.Step();
        }

        private void Breakout(Vector2Int position)
        {
            Debug.Assert(_shapeTravelLine.Contains(position));
            _shapeTravelBreakoutPosition = position;
            _actor.Direction = _shapeTravelLine.Direction.Turn(_shape.Turn.GetOpposite());

            _actor.Position = position;
            _actor.Step();
            Debug.Assert(!_shapeTravelLine.Contains(_actor.Position));

            _drawingLineChain.Clear();
            _drawingLineChain.Set(_shapeTravelBreakoutPosition, _actor.Position);

            _movementMode = PlayerMovementMode.Drawing;
        }

        private void MovePlayerDrawing()
        {
            _actor.Step();

            if (_drawingLineChain.Contains(_actor.Position, line => line.Direction.GetOrientation() != _actor.Direction.GetOrientation()))
            {
                // reset drawing
                Breakout(_shapeTravelBreakoutPosition);
                return;
            }

            _drawingLineChain.Extend(_actor.Position);

            // lines the player can collide with must be directed like the player direction, but turned left/right opposite of the shape turn
            GridDirection collisionLinesDirection = _actor.Direction.Turn(_shape.Turn.GetOpposite());

            DeprecatedLine shapeCollisionLine = _shape.FindLineAt(_actor.Position, line => line.Direction == collisionLinesDirection);
            if (shapeCollisionLine)
            {
                DiscontinueDrawing(shapeCollisionLine);
            }
        }

        private void DiscontinueDrawing(DeprecatedLine shapeCollisionLine)
        {
            DeprecatedLine newShapeTravelLine = _drawingLineChain.End;
            _isTravelingInShapeDirection = _shape.Incorporate(_drawingLineChain, _shapeTravelLine, shapeCollisionLine);
#if DEBUG
            Debug.Assert(_shape.OutlineContains(newShapeTravelLine));
#endif
            _shapeTravelLine = newShapeTravelLine;
            _actor.Direction = _shapeTravelLine.GetDirection(_isTravelingInShapeDirection);
            _movementMode = PlayerMovementMode.ShapeTravel;
        }
    }
}