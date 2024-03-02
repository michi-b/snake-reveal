using Game.Simulation;

namespace Game.State
{
    public class SimulationRunningState : IGameState
    {
        private readonly Game _game;

        public GameStateId Id => GameStateId.SimulationRunning;

        public SimulationRunningState(Game game)
        {
            _game = game;
        }

        public IGameState FixedUpdate()
        {
            if (_game.GameMenuState.TryEnter(out GameMenuState enteredGameMenuState))
            {
                return enteredGameMenuState;
            }

            SimulationUpdateResult updateResult = _game.Simulation.SimulationUpdate();

            return updateResult.PlayerDidCollideWithEnemy || updateResult.PlayerDidCollideWithDrawing
                ? _game.WaitingForSimulationInputState.Enter()
                : this;
        }

        public IGameState Enter()
        {
            _game.Simulation.Resume();
            return this;
        }
    }
}