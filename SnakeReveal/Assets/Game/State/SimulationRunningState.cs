using Game.Simulation;

namespace Game.State
{
    public class SimulationRunningState : GameState
    {
        public override GameStateId Id => GameStateId.SimulationRunning;

        public SimulationRunningState(Game game) : base(game)
        {
        }

        public override IGameState FixedUpdate()
        {
            if(TryEnterCommonState(out IGameState newState))
            {
                return newState;
            }

            SimulationUpdateResult updateResult = Game.Simulation.SimulationUpdate();

            if (updateResult.LevelComplete)
            {
                return Game.LevelCompleteState.Enter();
            }
            
            return updateResult.PlayerDidCollideWithEnemy || updateResult.PlayerDidCollideWithDrawing
                ? Game.WaitingForSimulationInputState.Enter()
                : this;
        }

        public IGameState Enter()
        {
            Game.Simulation.Resume();
            return this;
        }
    }
}