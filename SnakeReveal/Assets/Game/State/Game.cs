using System;
using Game.Gui.AvailableDirectionsIndication;
using Game.Gui.GameMenu;
using UnityEngine;

namespace Game.State
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private Simulation _simulation;
        [SerializeField] private GameMenu _gameMenu;
        [SerializeField] private AvailableDirectionsIndication _availableDirectionsIndication;

        private IGameState _currentState;
        private GameMenuState _gameMenuState;
        private WaitingForSimulationInputState _waitingForSimulationInputState;
        private SimulationRunningState _simulationRunningState;

        public Simulation Simulation => _simulation;
        public GameMenu Menu => _gameMenu;

        public GameMenuState GameMenuState => _gameMenuState;
        public WaitingForSimulationInputState WaitingForSimulationInputState => _waitingForSimulationInputState;
        public SimulationRunningState RunningState => _simulationRunningState;


        protected virtual void Awake()
        {
            _gameMenuState = new GameMenuState(this);
            _waitingForSimulationInputState = new WaitingForSimulationInputState(this, _availableDirectionsIndication);
            _simulationRunningState = new SimulationRunningState(this);
        }

        protected void Start()
        {
            _currentState = _waitingForSimulationInputState.Enter();
        }

        protected virtual void FixedUpdate()
        {
            _currentState = _currentState.FixedUpdate();
        }
    }
}