using System;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Lines;
using Game.Lines.Insertion;
using Game.Player;
using Game.Simulation.Grid;
using UnityEngine;

namespace Game.Simulation.States
{
    public class DrawingState : IPlayerSimulationState
    {
        private Line _shapeBreakoutLine;
        private Vector2Int _shapeBreakoutPosition;
        private Turn _lastBoundsTravelTurn;

        private readonly PlayerSimulation _simulation;

        private SimulationGrid Grid => _simulation.Grid;
        private PlayerActor Actor => _simulation.Actor;
        private DrawingChain Drawing => _simulation.Drawing;
        private DrawnShape Shape => _simulation.Shape;

        public string Name => "Drawing";

        public DrawingState(PlayerSimulation simulation)
        {
            _simulation = simulation;
            ClearState();
        }

        public IPlayerSimulationState Update(GridDirection requestedDirection, ref SimulationUpdateResult result)
        {
            if (requestedDirection != GridDirection.None
                && requestedDirection != Actor.Direction
                && requestedDirection != Actor.Direction.Reverse()
                && Actor.GetCanMoveInGridBounds(requestedDirection))
            {
                Actor.Direction = requestedDirection;
            }

            return Move(ref result);
        }

        public IPlayerSimulationState EnterDrawingAndMove(Line shapeBreakoutLine, Vector2Int breakoutPosition, GridDirection breakoutDirection, ref SimulationUpdateResult result)
        {
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

            if (TryReconnect(out ShapeTravelState enteredShapeTravelState, ref result))
            {
                return enteredShapeTravelState;
            }

            return this;
        }

        private IPlayerSimulationState Move(ref SimulationUpdateResult result)
        {
            if (!Actor.TryMoveCheckingEnemies())
            {
                result.PlayerDidCollideWithEnemy = true;
                return Reset();
            }

            //collision with drawing line
            if (Drawing.Contains(Actor.Position))
            {
                result.PlayerDidCollideWithDrawing = true;
                return Reset();
            }

            ExtendDrawingToActor();

            GridSide boundsSide = Grid.GetBoundsSide(Actor.Position);
            if (boundsSide != GridSide.None)
            {
                // actor might be forced to turn on bounds to avoid him moving out of bounds next step

                GridCorner boundsCorner = Grid.GetBoundsCorner(Actor.Position);
                if (boundsCorner != GridCorner.None)
                {
                    // if actor is exactly on a corner, turn inside the corner
                    Actor.Direction = Actor.Direction.TurnInsideCorner(boundsCorner);
                }
                else
                {
                    if (Actor.Direction.GetAxis() != boundsSide.GetLineAxis())
                    {
                        Actor.Direction = _lastBoundsTravelTurn != Turn.None
                            // if actor was on bounds before, turn that way, that he cannot trap himself in a drawing loop
                            ? boundsSide.GetDirection(_lastBoundsTravelTurn)
                            // direction on other axis can be chosen => assign latest direction on other axis
                            : Actor.GetLatestDirection(Actor.Direction.GetAxis().GetOther());
                    }
                }
            }

            if (TryReconnect(out ShapeTravelState enteredShapeTravelState, ref result))
            {
                ClearState();
                return enteredShapeTravelState;
            }

            return this;
        }

        private IPlayerSimulationState Reset()
        {
            Actor.Position = Drawing.StartPosition;
            ClearState();
            return _simulation.ShapeTravelState.ReEnter();
        }

        public GridDirections GetAvailableDirections()
        {
            var result = GridDirections.All;
            result = result.WithoutDirection(Drawing.LastLine.GetDirection(false));
            result = Actor.RestrictDirectionsToAvailableInBounds(result);
            return result;
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
        /// update <see cref="_lastBoundsTravelTurn"/> if last line was traveling on bounds
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void TrackBoundsInteraction()
        {
            // if player was traveling on bounds before, he is forced to always travel in that same turn (until reconnection)
            // because otherwise he would trap himself
            if (_lastBoundsTravelTurn != Turn.None)
            {
                return;
            }

            Line lastLine = Drawing.LastLine;
            if (lastLine.GetBoundsInteraction() == Line.BoundsInteraction.Exit)
            {
                Line boundsLine = lastLine.Previous;
#if DEBUG
                Debug.Assert(boundsLine != null, "boundsLine != null");
#endif

                GridSide gridSide = Grid.GetBoundsSide(boundsLine);

                bool isClockwise = gridSide switch
                {
                    GridSide.None => throw new ArgumentOutOfRangeException(),
                    GridSide.Bottom => boundsLine.Direction == GridDirection.Left,
                    GridSide.Left => boundsLine.Direction == GridDirection.Up,
                    GridSide.Top => boundsLine.Direction == GridDirection.Right,
                    GridSide.Right => boundsLine.Direction == GridDirection.Down,
                    _ => throw new ArgumentOutOfRangeException()
                };
                _lastBoundsTravelTurn = isClockwise ? Turn.Right : Turn.Left;
            }
        }

        private bool TryReconnect(out ShapeTravelState enteredShapeTravelState, ref SimulationUpdateResult result)
        {
            if (Shape.TryGetReconnectionLine(Actor, out Line shapeCollisionLine))
            {
                // insert drawing into shape and switch to shape travel
                InsertionResult insertionResult = Shape.Insert(Drawing, _shapeBreakoutLine, shapeCollisionLine);
                enteredShapeTravelState = _simulation.ShapeTravelState.Enter(insertionResult, ref result);
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
            _lastBoundsTravelTurn = Turn.None;
        }
    }
}