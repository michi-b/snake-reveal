﻿namespace Game.State
{
    public abstract class GameState : IGameState
    {
        protected readonly Game Game;

        public abstract GameStateId Id { get; }

        protected GameState(Game game)
        {
            Game = game;
        }

        public abstract IGameState FixedUpdate();

        protected bool TryEnterCommonState(out IGameState enteredState)
        {
            if (Id.GetIsGameMenuAvailable() && Game.GameMenuState.TryEnter(out GameMenuState enteredGameMenuState))
            {
                enteredState = enteredGameMenuState;
                return true;
            }

            enteredState = null;
            return false;
        }
    }
}