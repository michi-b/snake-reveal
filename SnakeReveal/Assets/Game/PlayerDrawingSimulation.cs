using System;
using Game.Enums;
using Game.Lines;
using Game.Player;
using JetBrains.Annotations;
using UnityEngine;

namespace Game
{
    public class PlayerDrawingSimulation
    {
        private readonly PlayerActor _actor;
        private readonly PlayerActorControls _controls;

        private readonly LineChain _drawingLineChain;

        private readonly LineLoop _shape;
        private readonly Simulation _simulation;

        // whether the player in shape travel mode is traveling in the same direction as the shape turn
        private bool _isTravelingInShapeDirection;

        private int _lastDirectionChangeRequestTick = -1;

        private PlayerMovementMode _movementMode;

        private Vector2Int _shapeTravelBreakoutPosition;

        [NotNull] private Line _shapeTravelLine;


        public PlayerDrawingSimulation(Simulation simulation, PlayerActor actor, LineLoop shape, LineChain drawingLineChain)
        {
            _simulation = simulation;
            _actor = actor;
            _shape = shape;
            _drawingLineChain = drawingLineChain;
            _controls = new PlayerActorControls(RequestDirectionChange);
            _shapeTravelLine = _shape.TryGetFirstLineAt(_actor.Position, out Line line) ? line : throw new InvalidOperationException("Actor is not on shape");
            _actor.Direction = _shapeTravelLine.Direction;
            _isTravelingInShapeDirection = true;
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

        private Turn TravelTurn => _isTravelingInShapeDirection ? _shape.Turn : _shape.Turn.Reverse();

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

            // todo: apply grid position only once per frame instead (and extrapolate)
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
#if DEBUG
            Debug.Assert(_shapeTravelLine.Contains(_actor.Position));
#endif

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
                Line newLine = _shapeTravelLine.GetNext(_isTravelingInShapeDirection);
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
            _actor.Direction = _shapeTravelLine.Direction.Turn(_shape.Turn.Reverse());

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

            // todo: can't I just check the current shape travel line?
            if (_drawingLineChain.ContainsLineAt(_actor.Position))
            {
                // reset drawing
                Breakout(_shapeTravelBreakoutPosition);
                return;
            }

            _drawingLineChain.Extend(_actor.Position);

            // lines the player can collide with must be directed like the player direction, but turned left/right opposite of the shape turn
            GridDirection collisionLinesDirection = _actor.Direction.Turn(_shape.Turn.Reverse());
            Line shapeCollisionLine = _shape.GetLineAt(_actor.Position, collisionLinesDirection);
            if (shapeCollisionLine)
            {
                DiscontinueDrawing(shapeCollisionLine);
            }
        }

        private void DiscontinueDrawing(Line shapeCollisionLine)
        {
            _isTravelingInShapeDirection = _shape.Insert(_drawingLineChain, _shapeTravelLine, shapeCollisionLine);

            _drawingLineChain.Clear();

            _shapeTravelLine = _shape.Start;
#if DEBUG
            Debug.Assert(_shape.Start.Contains(_actor.Position));
#endif
            _actor.Direction = _shapeTravelLine.GetDirection(_isTravelingInShapeDirection);
            _movementMode = PlayerMovementMode.ShapeTravel;
        }
    }
}