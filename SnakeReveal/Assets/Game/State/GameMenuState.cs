namespace Game.State
{
    public class GameMenuState : GameState
    {
        public override GameStateId Id => GameStateId.GameMenu;

        public GameMenuState(Game game) : base(game)
        {
        }

        public override IGameState FixedUpdate()
        {
            return Game.Menu.AnimatorState.IsSomewhatOpen
                ? this
                : Game.WaitingForSimulationInputState.Enter();
        }

        public bool TryEnter(out GameMenuState enteredState)
        {
            if (Game.Menu.AnimatorState.IsSomewhatOpen)
            {
                enteredState = this;
                return true;
            }

            enteredState = null;
            return false;
        }
    }
}