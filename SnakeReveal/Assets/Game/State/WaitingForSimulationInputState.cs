using Game.Enums;
using Game.Enums.Extensions;
using Game.Gui.AvailableDirectionsIndication;
using Game.Player;

namespace Game.State
{
    public class WaitingForSimulationInputState : GameState
    {
        private readonly AvailableDirectionsIndication _availableDirectionsIndication;
        private GridDirections _availableDirections;

        public override GameStateId Id => GameStateId.WaitingForSimulationInput;

        public WaitingForSimulationInputState(Game game, AvailableDirectionsIndication availableDirectionsIndication)
            : base(game)
        {
            _availableDirectionsIndication = availableDirectionsIndication;
        }

        public override IGameState FixedUpdate()
        {
            if (TryEnterCommonState(out IGameState newState))
            {
                return newState;
            }

            Simulation.GameSimulation simulation = Game.Simulation;
            GridDirection requestedDirection = simulation.GetRequestedDirection();
            if (requestedDirection != GridDirection.None)
            {
                if (_availableDirections.Contains(requestedDirection))
                {
                    simulation.Player.Direction = requestedDirection;
                    Exit();
                    return Game.RunningState.Enter();
                }
            }

            return this;
        }

        public IGameState Enter()
        {
            Simulation.GameSimulation simulation = Game.Simulation;
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