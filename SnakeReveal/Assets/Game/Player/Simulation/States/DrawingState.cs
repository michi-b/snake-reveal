using System;
using System.Diagnostics;
using Game.Enums;
using Game.Grid;
using Game.Lines;
using Game.Lines.Insertion;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Player.Simulation.States
{
    public class DrawingState : IPlayerSimulationState
    {
        private readonly SimulationGrid _grid;
        private readonly PlayerActor _actor;
        private readonly DrawingChain _drawing;
        private readonly DrawnShape _shape;
        private readonly ShapeTravelState _shapeTravelState;
        private Line _shapeBreakoutLine;
        private Vector2Int _shapeBreakoutPosition;
        private int? _clockwiseTurnWeightSinceLastLineOnBounds;

        public DrawingState(SimulationGrid grid, PlayerActor actor, DrawnShape shape, DrawingChain drawing, ShapeTravelState shapeTravelState)
        {
            _grid = grid;
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
                && requestedDirection != _actor.Direction.Reverse()
                && _actor.GetCanMoveInGridBounds(requestedDirection))
            {
                _actor.Direction = requestedDirection;
            }

            return Move();
        }

        public IPlayerSimulationState EnterDrawingAndMove(Line shapeBreakoutLine, Vector2Int breakoutPosition, GridDirection breakoutDirection)
        {
            ClearState();

            _shapeBreakoutLine = shapeBreakoutLine;
            _actor.Position = _shapeBreakoutPosition = breakoutPosition;
            _actor.Direction = breakoutDirection;

            _drawing.Activate(_shapeBreakoutPosition, _actor.Position);

            // note that enemy collisions are not checked on first move, to avoid an infinite loop of re-entering drawing state
            _actor.Move();

#if DEBUG
            Debug.Assert(!_drawing.Contains(_actor.Position));
#endif

            ExtendDrawingToActor();

            return TryReconnect();
        }

        private IPlayerSimulationState Move()
        {
            if (!_actor.TryMoveCheckingEnemies())
            {
                return Reset();
            }

            //collision with drawing line
            if (_drawing.Contains(_actor.Position))
            {
                return Reset();
            }

            ExtendDrawingToActor();

            if (!_actor.GetCanMoveInGridBounds(_actor.Direction))
            {
                _actor.TurnOnGridBounds();
            }

            return TryReconnect();
        }

        private void ExtendDrawingToActor()
        {
            _drawing.Extend(_actor.Position, out bool turned);

            if (turned)
            {
                // increment clockwise turn weight since last line on bounds
                IncrementTrackClockwiseTurnWeightSinceLastLineOnBounds();
            }
        }

        private void IncrementTrackClockwiseTurnWeightSinceLastLineOnBounds()
        {
            switch (_drawing.LastLine.GetBoundsInteraction())
            {
                case Line.BoundsInteraction.None:
                    if (_clockwiseTurnWeightSinceLastLineOnBounds != null)
                    {
                        _clockwiseTurnWeightSinceLastLineOnBounds += _drawing.LastLine.GetClockwiseTurnWeightFromPrevious();
                        LogClockwiseTurnWeightSinceLastLineOnBounds();
                    }

                    break;
                case Line.BoundsInteraction.Exit:
                    _clockwiseTurnWeightSinceLastLineOnBounds = 0;
                    LogClockwiseTurnWeightSinceLastLineOnBounds();
                    break;
                case Line.BoundsInteraction.Enter:
                case Line.BoundsInteraction.OnBounds:
                    _clockwiseTurnWeightSinceLastLineOnBounds = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;

            void LogClockwiseTurnWeightSinceLastLineOnBounds()
            {
                Debug.Log($"Clockwise turn weight since last line on bounds: {_clockwiseTurnWeightSinceLastLineOnBounds}");
            }
        }

        private IPlayerSimulationState Reset()
        {
            return EnterDrawingAndMove(_shapeBreakoutLine, _drawing.StartPosition, _drawing.StartDirection);
        }

        private IPlayerSimulationState TryReconnect()
        {
            if (_shape.TryGetReconnectionLine(_actor, out Line shapeCollisionLine))
            {
                // insert drawing into shape and switch to shape travel
                InsertionResult insertionResult = _shape.Insert(_drawing, _shapeBreakoutLine, shapeCollisionLine);
                return _shapeTravelState.Enter(insertionResult);
            }

            return this;
        }

        private void ClearState()
        {
            _drawing.Deactivate();
            _shapeBreakoutLine = null;
            _shapeBreakoutPosition = new Vector2Int(-1, -1);
            _clockwiseTurnWeightSinceLastLineOnBounds = null;
        }
    }
}