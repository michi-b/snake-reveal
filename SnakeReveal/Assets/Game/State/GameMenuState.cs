namespace Game.State
{
    public class GameMenuState : GameState
    {
        public override GameStateId Id => GameStateId.GameMenu;
        protected override bool ArePlayerActorControlsEnabled => false;

        public GameMenuState(Game game) : base(game)
        {
        }

        public override IGameState FixedUpdate() =>
            Game.Gui.GameMenu.AnimatorState.IsSomewhatOpen
                ? this
                : Game.WaitingForSimulationInputState.Enter();

        public bool TryEnter(out GameMenuState enteredState)
        {
            if (Game.Gui.GameMenu.AnimatorState.IsSomewhatOpen)
            {
                OnEnter();
                enteredState = this;
                return true;
            }

            enteredState = null;
            return false;
        }
    }
}