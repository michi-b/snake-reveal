using Game.Enums;
using Game.Enums.Extensions;
using Game.Player;
using Game.UI.AvailableDirectionsIndication;

namespace Game.State
{
    public class WaitingForSimulationInputState : IGameState
    {
        private readonly Game _game;
        private readonly AvailableDirectionsIndication _availableDirectionsIndication;
        private GridDirections _availableDirections;

        public GameStateId Id => GameStateId.WaitingForSimulationInput;

        public WaitingForSimulationInputState(Game game, AvailableDirectionsIndication availableDirectionsIndication)
        {
            _game = game;
            _availableDirectionsIndication = availableDirectionsIndication;
        }

        public IGameState FixedUpdate()
        {
            if (_game.GameMenuState.TryEnter(out GameMenuState enteredGameMenuState))
            {
                Exit();
                return enteredGameMenuState;
            }

            Simulation.GameSimulation simulation = _game.Simulation;
            GridDirection requestedDirection = simulation.GetRequestedDirection();
            if (requestedDirection != GridDirection.None)
            {
                if (_availableDirections.Contains(requestedDirection))
                {
                    simulation.Player.Direction = requestedDirection;
                    Exit();
                    return _game.RunningState.Enter();
                }
            }

            return this;
        }

        public IGameState Enter()
        {
            Simulation.GameSimulation simulation = _game.Simulation;
            PlayerActor playerActor = simulation.Player;
            _availableDirectionsIndication.Place(playerActor.transform.localPosition);
            playerActor.Direction = GridDirection.None;
            _availableDirections = simulation.GetAvailableDirections();
            _availableDirectionsIndication.Directions = _availableDirections;
            _availableDirectionsIndication.SetVisible(true);
            return this;
        }

        private void Exit()
        {
            _availableDirectionsIndication.SetVisible(false);
        }
    }
}