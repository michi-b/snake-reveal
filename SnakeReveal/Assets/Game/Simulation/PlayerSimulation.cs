using Game.Player;
using Game.Player.Controls;
using Game.Simulation.Grid;
using Game.Simulation.States;
using UnityEngine;

namespace Game.Simulation
{
    public class PlayerSimulation
    {
        private readonly GameSimulation _game;

        public IPlayerSimulationState CurrentState { get; private set; }

        public int CoveredCellCount => ShapeTravelState.CoveredCellCount;

        public IPlayerActorControls Controls { get; }

        public PlayerActor Actor => _game.Player;

        public DrawnShape Shape => _game.DrawnShape;

        public DrawingChain Drawing => _game.Drawing;

        public SimulationGrid Grid => _game.Grid;

        public DrawingState DrawingState { get; }

        public ShapeTravelState ShapeTravelState { get; }

        public void Move(ref SimulationUpdateResult result)
        {
            for (int moveIndex = 0; moveIndex < _game.Player.Speed; moveIndex++)
            {
                CurrentState = CurrentState.Update(Controls.GetRequestedDirection(), ref result);
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

            ShapeTravelState = new ShapeTravelState(this);
            DrawingState = new DrawingState(this);

            CurrentState = ShapeTravelState.Initialize();
        }

        public void Resume()
        {
            CurrentState.Resume();
        }
    }
}