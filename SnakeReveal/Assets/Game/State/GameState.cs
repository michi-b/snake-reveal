namespace Game.State
{
    public abstract class GameState : IGameState
    {
        protected readonly Game Game;

        public abstract GameStateId Id { get; }

        protected abstract bool ArePlayerActorControlsEnabled { get; }

        protected GameState(Game game)
        {
            Game = game;
        }

        public abstract IGameState FixedUpdate();

        protected bool TryEnterGameMenuState(out IGameState enteredState)
        {
            if (Id.GetIsGameMenuAvailable() && Game.GameMenuState.TryEnter(out GameMenuState enteredGameMenuState))
            {
                enteredState = enteredGameMenuState;
                return true;
            }

            enteredState = null;
            return false;
        }

        protected virtual void OnEnter()
        {
            Game.PlayerActorControls.IsEnabled = ArePlayerActorControlsEnabled;
        }
    }
}