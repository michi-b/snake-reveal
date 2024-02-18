using Game.Grid;
using Game.Player;
using Game.Player.Controls;
using Game.Simulation.States;
using UnityEngine;

namespace Game.Simulation
{
    public class PlayerSimulation
    {
        private readonly GameSimulation _game;

        private readonly ShapeTravelState _shapeTravelState;
        private readonly DrawingState _drawingState;

        public IPlayerSimulationState CurrentState { get; private set; }

        public int CoveredCellCount => ShapetravelState.CoveredCellCount;

        public IPlayerActorControls Controls { get; }

        public GameSimulation Game => _game;

        public PlayerActor Actor => _game.Player;

        public DrawnShape Shape => _game.DrawnShape;

        public DrawingChain Drawing => _game.Drawing;

        public SimulationGrid Grid => _game.Grid;

        public DrawingState DrawingState => _drawingState;

        public ShapeTravelState ShapetravelState => _shapeTravelState;

        public void Move(ref SimulationUpdateResult result)
        {
            for (int moveIndex = 0; moveIndex < _game.Player.Speed; moveIndex++)
            {
                CurrentState = CurrentState.Move(Controls.GetRequestedDirection(), ref result);
            }

            // todo: apply grid position only once per frame instead (and extrapolate)
            _game.Player.ApplyPosition();
        }

        public PlayerSimulation(GameSimulation simulation, bool monkeyTestPlayerSimulationWithRandomInputs)
        {
            _game = simulation;

            SimulationGrid grid = simulation.Grid;
            PlayerActor actor = simulation.Player;
            DrawnShape shape = simulation.DrawnShape;
            DrawingChain drawing = simulation.Drawing;
            Debug.Assert(grid != null && actor.Grid == grid && shape.Grid == grid && drawing.Grid == grid);
            Controls = monkeyTestPlayerSimulationWithRandomInputs ? new MonkeyTestRandomInputPlayerActorControls() : PlayerActorControls.Create();
            Controls.Activate();

            _shapeTravelState = new ShapeTravelState(this);
            _drawingState = new DrawingState(this);

            CurrentState = ShapetravelState.Initialize();
        }
    }
}