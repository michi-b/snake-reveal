using Game.Gui.GameInfo.LevelCompleteMessage;
using Game.Simulation;

namespace Game.State
{
    public class LevelCompleteState : GameState
    {
        public LevelCompleteState(Game game) : base(game)
        {
        }

        protected override bool ArePlayerActorControlsEnabled => false;

        public LevelCompleteState Enter()
        {
            GameSimulation simulation = Game.Simulation;

            float simulationTime = simulation.GetSimulationTime();
            float percentCompletion = simulation.GetPercentCompletion();

            LevelCompleteMessage levelCompleteMessage = Game.Gui.GameInfo.LevelCompleteMessage;
            levelCompleteMessage.Show(simulationTime, percentCompletion);

            OnEnter();

            return this;
        }

        public override IGameState FixedUpdate() => this; // cannot exit Level Completed state

        public override GameStateId Id => GameStateId.LevelComplete;
    }
}