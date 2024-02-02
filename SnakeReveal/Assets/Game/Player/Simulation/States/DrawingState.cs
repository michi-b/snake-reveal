using System;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Grid;
using Game.Lines;
using Game.Lines.Insertion;
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
        private int _clockwiseTurnWeightSinceLastBounds;
        private GridSide _lastBoundsSide;

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

            if (TryReconnect(out ShapeTravelState enteredShapeTravelState))
            {
                return enteredShapeTravelState;
            }

            return this;
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

            GridSide boundsSide = _grid.GetBoundsSide(_actor.Position);
            if (boundsSide != GridSide.None)
            {
                // actor needs to turn, because next move would move him out of bounds
                GridCorner boundsCorner = _grid.GetBoundsCorner(_actor.Position);

                if (boundsCorner != GridCorner.None)
                {
                    // if actor is exactly on a corner, turn inside the corner
                    _actor.Direction = _actor.Direction.TurnInsideCorner(boundsCorner);
                }
                else if (_actor.Direction.GetAxis() != boundsSide.GetLineAxis())
                {
                    if (_lastBoundsSide != GridSide.None)
                    {
                        // if actor was on bounds before, turn that way, that he cannot trap himself in a drawing loop
                        int boundsTurnWeight = _lastBoundsSide.GetClockwiseTurnWeight(boundsSide);
                        int relativeTurnWeight = _clockwiseTurnWeightSinceLastBounds - boundsTurnWeight;
                        _actor.Direction = relativeTurnWeight switch
                        {
                            2 => _actor.Direction.Turn(Turn.Left),
                            -2 => _actor.Direction.Turn(Turn.Right),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    }
                    else
                    {
                        // direction on other axis can be chosen => assign latest direction on other axis
                        _actor.Direction = _actor.GetLatestDirection(_actor.Direction.GetAxis().GetOther());
                    }
                }
            }

            if (TryReconnect(out ShapeTravelState enteredShapeTravelState))
            {
                return enteredShapeTravelState;
            }

            return this;
        }

        private void ExtendDrawingToActor()
        {
            _drawing.Extend(_actor.Position, out bool turned);

            if (turned)
            {
                // increment clockwise turn weight since last line on bounds
                TrackBounds();
            }
        }

        /// <summary>
        /// update <see cref="_lastBoundsSide"/> and <see cref="_clockwiseTurnWeightSinceLastBounds"/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void TrackBounds()
        {
            switch (_drawing.LastLine.GetBoundsInteraction())
            {
                case Line.BoundsInteraction.None:
                    if (_lastBoundsSide != GridSide.None)
                    {
                        _clockwiseTurnWeightSinceLastBounds += _drawing.LastLine.GetClockwiseTurnWeightFromPrevious();
                        LogBoundsInteraction();
                    }

                    break;
                case Line.BoundsInteraction.Exit:
                    _clockwiseTurnWeightSinceLastBounds = 0;
                    _lastBoundsSide = _grid.GetBoundsSide(_drawing.LastLine.Start);
                    Debug.Assert(_lastBoundsSide != GridSide.None);
                    LogBoundsInteraction();
                    break;
                case Line.BoundsInteraction.Enter:
                case Line.BoundsInteraction.OnBounds:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;

            void LogBoundsInteraction()
            {
                Debug.Log($"Clockwise turn weight since last line on {_lastBoundsSide.ToString()} bounds: {_clockwiseTurnWeightSinceLastBounds}");
            }
        }

        private IPlayerSimulationState Reset()
        {
            return EnterDrawingAndMove(_shapeBreakoutLine, _drawing.StartPosition, _drawing.StartDirection);
        }

        private bool TryReconnect(out ShapeTravelState enteredShapeTravelState)
        {
            if (_shape.TryGetReconnectionLine(_actor, out Line shapeCollisionLine))
            {
                // insert drawing into shape and switch to shape travel
                InsertionResult insertionResult = _shape.Insert(_drawing, _shapeBreakoutLine, shapeCollisionLine);
                enteredShapeTravelState = _shapeTravelState.Enter(insertionResult);
                return true;
            }

            enteredShapeTravelState = null;
            return false;
        }

        private void ClearState()
        {
            _drawing.Deactivate();
            _shapeBreakoutLine = null;
            _shapeBreakoutPosition = new Vector2Int(-1, -1);
            _clockwiseTurnWeightSinceLastBounds = 0;
            _lastBoundsSide = GridSide.None;
        }
    }
}