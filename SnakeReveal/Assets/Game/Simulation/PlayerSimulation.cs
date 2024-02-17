using Game.Grid;
using Game.Player;
using Game.Player.Controls;
using Game.Simulation.States;
using UnityEngine;

namespace Game.Simulation
{
    public class PlayerSimulation
    {
        private readonly PlayerActor _actor;

        // whether the player in shape travel mode is traveling in the same direction as the shape turn

        private readonly ShapeTravelState _shapeTravelState;

        public IPlayerSimulationState CurrentState { get; private set; }

        public int CoveredCellCount => _shapeTravelState.CoveredCellCount;

        public IPlayerActorControls Controls { get; }

        public void Move()
        {
            for (int moveIndex = 0; moveIndex < _actor.Speed; moveIndex++)
            {
                CurrentState = CurrentState.Move(Controls.GetRequestedDirection());
            }

            // todo: apply grid position only once per frame instead (and extrapolate)
            _actor.ApplyPosition();
        }

        public PlayerSimulation(Simulation simulation, bool monkeyTestPlayerSimulationWithRandomInputs)
        {
            SimulationGrid grid = simulation.Grid;
            PlayerActor actor = simulation.PlayerActor;
            DrawnShape shape = simulation.DrawnShape;
            DrawingChain drawing = simulation.Drawing;
            Debug.Assert(grid != null && actor.Grid == grid && shape.Grid == grid && drawing.Grid == grid);
            _actor = actor;
            Controls = monkeyTestPlayerSimulationWithRandomInputs ? new MonkeyTestRandomInputPlayerActorControls() : PlayerActorControls.Create();
            Controls.Activate();
            _shapeTravelState = new ShapeTravelState(_actor, shape);
            var drawingState = new DrawingState(grid, _actor, shape, drawing, _shapeTravelState);
            CurrentState = _shapeTravelState.Initialize(drawingState);
        }
    }
}