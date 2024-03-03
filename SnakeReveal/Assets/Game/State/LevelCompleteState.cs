using Game.Simulation;
using Game.UI.GameInfo.LevelCompleteMessage;

namespace Game.State
{
    public class LevelCompleteState : GameState
    {
        public LevelCompleteState(Game game) : base(game)
        {
        }

        public LevelCompleteState Enter()
        {
            GameSimulation simulation = Game.Simulation;

            float simulationTime = simulation.GetSimulationTime();
            float percentCompletion = simulation.GetPercentCompletion();

            LevelCompleteMessage levelCompleteMessage = Game.InfoGui.LevelCompleteMessage;
            levelCompleteMessage.Show(simulationTime, percentCompletion);

            return this;
        }

        public override IGameState FixedUpdate() => TryEnterCommonState(out IGameState newState) ? newState : this; // cannot exit Level Completed state

        public override GameStateId Id => GameStateId.LevelComplete;
    }
}