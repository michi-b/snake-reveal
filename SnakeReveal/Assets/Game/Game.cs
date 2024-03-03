using Game.Gui;
using Game.State;
using UnityEngine;

namespace Game
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private Simulation.GameSimulation _simulation;
        [SerializeField] private GuiContainer _gui;

        private IGameState _currentState;

        public Simulation.GameSimulation Simulation => _simulation;

        public GuiContainer Gui => _gui;

        public GameMenuState GameMenuState { get; private set; }

        public WaitingForSimulationInputState WaitingForSimulationInputState { get; private set; }

        public SimulationRunningState RunningState { get; private set; }

        public LevelCompleteState LevelCompleteState { get; private set; }

        protected virtual void Awake()
        {
            GameMenuState = new GameMenuState(this);
            WaitingForSimulationInputState = new WaitingForSimulationInputState(this, Gui.AvailableDirectionsIndication);
            RunningState = new SimulationRunningState(this);
            LevelCompleteState = new LevelCompleteState(this);
        }

        protected void Start()
        {
            _currentState = WaitingForSimulationInputState.Enter();

#if DEBUG
            Gui.DebugInfo.GameState = _currentState.Id.GetDisplayName();
#endif
        }

        protected virtual void FixedUpdate()
        {
            GameStateId originalState = _currentState.Id;
            _currentState = _currentState.FixedUpdate();
            if (_currentState.Id != originalState)
            {
#if DEBUG
                Gui.DebugInfo.GameState = _currentState.Id.GetDisplayName();
#endif
                ApplyIsGameMenuAvailable();
            }
        }

        private void ApplyIsGameMenuAvailable()
        {
            Gui.GameMenu.IsOpenButtonEnabled = _currentState.Id.GetIsGameMenuAvailable();
        }
    }
}