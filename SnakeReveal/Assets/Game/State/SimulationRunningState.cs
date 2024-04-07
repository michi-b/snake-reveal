using Game.Simulation;

namespace Game.State
{
    public class SimulationRunningState : GameState
    {
        public override GameStateId Id => GameStateId.SimulationRunning;

        public SimulationRunningState(Game game) : base(game)
        {
            game.Simulation.IsRunning = false;
        }

        public override IGameState FixedUpdate()
        {
            if (TryEnterGameMenuState(out IGameState newState))
            {
                return Exit(newState);
            }

            SimulationUpdateResult updateResult = Game.Simulation.SimulationUpdate();

            if (updateResult.LevelComplete)
            {
                return Game.LevelCompleteState.Enter();
            }

            return updateResult.PlayerDidCollideWithEnemy || updateResult.PlayerDidCollideWithDrawing
                ? Exit(Game.WaitingForSimulationInputState.Enter())
                : this;
        }

        public IGameState Enter()
        {
            Game.Simulation.IsRunning = true;
            return this;
        }

        private IGameState Exit(IGameState newState = null)
        {
            Game.Simulation.IsRunning = false;
            return newState;
        }
    }
}