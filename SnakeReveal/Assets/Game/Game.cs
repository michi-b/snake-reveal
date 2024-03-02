using Game.State;
using Game.UI.AvailableDirectionsIndication;
using Game.UI.DebugInfo;
using Game.UI.GameMenu;
using UnityEngine;

namespace Game
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private Simulation.GameSimulation _simulation;
        [SerializeField] private GameMenu _gameMenu;
        [SerializeField] private AvailableDirectionsIndication _availableDirectionsIndication;
        [SerializeField] private DebugInfoGui _debugInfoGui;


        private IGameState _currentState;

        public Simulation.GameSimulation Simulation => _simulation;
        public GameMenu Menu => _gameMenu;

        public GameMenuState GameMenuState { get; private set; }

        public WaitingForSimulationInputState WaitingForSimulationInputState { get; private set; }

        public SimulationRunningState RunningState { get; private set; }


        protected virtual void Awake()
        {
            GameMenuState = new GameMenuState(this);
            WaitingForSimulationInputState = new WaitingForSimulationInputState(this, _availableDirectionsIndication);
            RunningState = new SimulationRunningState(this);
        }

        protected void Start()
        {
            _currentState = WaitingForSimulationInputState.Enter();
            _debugInfoGui.GameState = _currentState.Id.GetDisplayName();
        }

        protected virtual void FixedUpdate()
        {
            GameStateId originalState = _currentState.Id;
            _currentState = _currentState.FixedUpdate();
            if (_currentState.Id != originalState)
            {
                _debugInfoGui.GameState = _currentState.Id.GetDisplayName();
            }
        }
    }
}