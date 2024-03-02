namespace Game.State
{
    public class GameMenuState : IGameState
    {
        private readonly Game _game;

        public GameStateId Id => GameStateId.GameMenu;

        public GameMenuState(Game game)
        {
            _game = game;
        }

        public IGameState FixedUpdate() =>
            _game.Menu.AnimatorState.IsSomewhatOpen
                ? this
                : _game.WaitingForSimulationInputState.Enter();

        public bool TryEnter(out GameMenuState enteredState)
        {
            if (_game.Menu.AnimatorState.IsSomewhatOpen)
            {
                enteredState = this;
                return true;
            }

            enteredState = null;
            return false;
        }
    }
}