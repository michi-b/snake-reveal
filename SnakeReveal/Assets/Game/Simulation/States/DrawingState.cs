using System;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Grid;
using Game.Lines;
using Game.Lines.Insertion;
using Game.Player;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Simulation.States
{
    public class DrawingState : IPlayerSimulationState
    {
        private Line _shapeBreakoutLine;
        private Vector2Int _shapeBreakoutPosition;
        private int _clockwiseTurnWeightSinceLastBounds;
        private GridSide _lastBoundsSide;

        private readonly PlayerSimulation _simulation;

        private SimulationGrid Grid => _simulation.Grid;
        private PlayerActor Actor => _simulation.Actor;
        private DrawingChain Drawing => _simulation.Drawing;
        private DrawnShape Shape => _simulation.Shape;

        public DrawingState(PlayerSimulation simulation)
        {
            _simulation = simulation;
            ClearState();
        }

        public IPlayerSimulationState Move(GridDirection requestedDirection)
        {
            if (requestedDirection != GridDirection.None
                && requestedDirection != Actor.Direction
                && requestedDirection != Actor.Direction.Reverse()
                && Actor.GetCanMoveInGridBounds(requestedDirection))
            {
                Actor.Direction = requestedDirection;
            }

            return Move();
        }

        public IPlayerSimulationState EnterDrawingAndMove(Line shapeBreakoutLine, Vector2Int breakoutPosition, GridDirection breakoutDirection)
        {
            ClearState();

            _shapeBreakoutLine = shapeBreakoutLine;
            Actor.Position = _shapeBreakoutPosition = breakoutPosition;
            Actor.Direction = breakoutDirection;

            Drawing.Activate(_shapeBreakoutPosition, Actor.Position);

            // note that enemy collisions are not checked on first move, to avoid an infinite loop of re-entering drawing state
            Actor.Move();

#if DEBUG
            Debug.Assert(!Drawing.Contains(Actor.Position));
#endif

            ExtendDrawingToActor();

            if (TryReconnect(out ShapeTravelState enteredShapeTravelState))
            {
                return enteredShapeTravelState;
            }

            return this;
        }

        public GridDirections GetAvailableDirections()
        {
            var result = GridDirections.All;
            result = result.WithoutDirection(Drawing.LastLine.GetDirection(false));
            result = Actor.RestrictDirectionsToAvailableInBounds(result);
            return result;
        }

        public string Name => "Drawing";

        private IPlayerSimulationState Move()
        {
            if (!Actor.TryMoveCheckingEnemies())
            {
                return Reset();
            }

            //collision with drawing line
            if (Drawing.Contains(Actor.Position))
            {
                return Reset();
            }

            ExtendDrawingToActor();

            GridSide boundsSide = Grid.GetBoundsSide(Actor.Position);
            if (boundsSide != GridSide.None)
            {
                // actor needs to turn, because next move would move him out of bounds
                GridCorner boundsCorner = Grid.GetBoundsCorner(Actor.Position);

                if (boundsCorner != GridCorner.None)
                {
                    // if actor is exactly on a corner, turn inside the corner
                    Actor.Direction = Actor.Direction.TurnInsideCorner(boundsCorner);
                }
                else if (Actor.Direction.GetAxis() != boundsSide.GetLineAxis())
                {
                    if (_lastBoundsSide != GridSide.None)
                    {
                        // if actor was on bounds before, turn that way, that he cannot trap himself in a drawing loop
                        int boundsTurnWeight = _lastBoundsSide.GetClockwiseTurnWeight(boundsSide);
                        int relativeTurnWeight = _clockwiseTurnWeightSinceLastBounds - boundsTurnWeight;
                        Actor.Direction = relativeTurnWeight switch
                        {
                            2 => Actor.Direction.Turn(Turn.Left),
                            -2 => Actor.Direction.Turn(Turn.Right),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    }
                    else
                    {
                        // direction on other axis can be chosen => assign latest direction on other axis
                        Actor.Direction = Actor.GetLatestDirection(Actor.Direction.GetAxis().GetOther());
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
            Drawing.Extend(Actor.Position, out bool turned);

            if (turned)
            {
                // increment clockwise turn weight since last line on bounds
                TrackBoundsInteraction();
            }
        }

        /// <summary>
        /// update <see cref="_lastBoundsSide"/> and <see cref="_clockwiseTurnWeightSinceLastBounds"/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void TrackBoundsInteraction()
        {
            switch (Drawing.LastLine.GetBoundsInteraction())
            {
                case Line.BoundsInteraction.None:
                    if (_lastBoundsSide != GridSide.None)
                    {
                        _clockwiseTurnWeightSinceLastBounds += Drawing.LastLine.GetClockwiseTurnWeightFromPrevious();
                    }

                    break;
                case Line.BoundsInteraction.Exit:
                    _clockwiseTurnWeightSinceLastBounds = 0;
                    _lastBoundsSide = Grid.GetBoundsSide(Drawing.LastLine.Start);
                    Debug.Assert(_lastBoundsSide != GridSide.None);
                    break;
                case Line.BoundsInteraction.Enter:
                case Line.BoundsInteraction.OnBounds:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IPlayerSimulationState Reset() => EnterDrawingAndMove(_shapeBreakoutLine, Drawing.StartPosition, Drawing.StartDirection);

        private bool TryReconnect(out ShapeTravelState enteredShapeTravelState)
        {
            if (Shape.TryGetReconnectionLine(Actor, out Line shapeCollisionLine))
            {
                // insert drawing into shape and switch to shape travel
                InsertionResult insertionResult = Shape.Insert(Drawing, _shapeBreakoutLine, shapeCollisionLine);
                enteredShapeTravelState = _simulation.ShapetravelState.Enter(insertionResult);
                return true;
            }

            enteredShapeTravelState = null;
            return false;
        }

        private void ClearState()
        {
            Drawing.Deactivate();
            _shapeBreakoutLine = null;
            _shapeBreakoutPosition = new Vector2Int(-1, -1);
            _clockwiseTurnWeightSinceLastBounds = 0;
            _lastBoundsSide = GridSide.None;
        }
    }
}