namespace Game.State
{
    public class SimulationRunningState : IGameState
    {
        private readonly Game _game;

        public string Name => "SimulationRunning";

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

            _game.Simulation.SimulationUpdate();

            return this;
        }

        public IGameState Enter() => this;
    }
}