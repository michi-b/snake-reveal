using Game.Enums;
using Game.Enums.Extensions;
using Game.Player;
using Game.Simulation.Grid;
using Game.Simulation.States;
using UnityEngine;

namespace Game.Simulation
{
    public class PlayerSimulation
    {
        private readonly Game _game;
        private readonly GameSimulation _simulation;
        private bool _isRunning;

        public IPlayerSimulationState CurrentState { get; private set; }

        public int CoveredCellCount => ShapeTravelState.CoveredCellCount;

        public PlayerActor Actor => _simulation.Player;

        public DrawnShape Shape => _simulation.DrawnShape;

        public DrawingChain Drawing => _simulation.Drawing;

        public SimulationGrid Grid => _simulation.Grid;

        public DrawingState DrawingState { get; }

        public ShapeTravelState ShapeTravelState { get; }

        public void Move(ref SimulationUpdateResult result)
        {
            for (int moveIndex = 0; moveIndex < _simulation.Player.Speed; moveIndex++)
            {
                GridDirections validInputDirections = CurrentState.GetAvailableDirections().WithoutDirection(Actor.Direction);
                GridDirection inputDirection = _game.GetInputDirection(validInputDirections);

                if (inputDirection != GridDirection.None)
                {
#if DEBUG
                    Debug.Assert(validInputDirections.Contains(inputDirection));
#endif
                    Actor.Direction = inputDirection;
                }

                CurrentState = CurrentState.Update(ref result);
            }

            // todo: apply grid position only once per frame instead (and extrapolate)
            _simulation.Player.ApplyPosition();
        }

        public PlayerSimulation(Game game)
        {
            _game = game;
            _simulation = game.Simulation;

            SimulationGrid grid = _simulation.Grid;
            PlayerActor actor = _simulation.Player;
            DrawnShape shape = _simulation.DrawnShape;
            DrawingChain drawing = _simulation.Drawing;
            Debug.Assert(grid != null && actor.Grid == grid && shape.Grid == grid && drawing.Grid == grid);

            ShapeTravelState = new ShapeTravelState(this);
            DrawingState = new DrawingState(this);

            CurrentState = ShapeTravelState.Initialize();
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;

                    if (_isRunning)
                    {
                        CurrentState.Resume();
                    }
                }
            }
        }
    }
}