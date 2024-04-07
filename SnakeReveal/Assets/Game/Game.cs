using CustomPropertyDrawers;
using Game.Enums;
using Game.Gui;
using Game.Player.Controls;
using Game.Settings;
using Game.State;
using UnityEngine;

namespace Game
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private Simulation.GameSimulation _simulation;
        [SerializeField] private GuiContainer _gui;
        [SerializeField] private GameSettingsContainer _gameSettings;

        [SerializeField, ToggleLeft] private bool _monkeyTestPlayerSimulationWithRandomInputs;

        public Simulation.GameSimulation Simulation => _simulation;

        public GuiContainer Gui => _gui;

        public GameMenuState GameMenuState { get; private set; }

        public WaitingForSimulationInputState WaitingForSimulationInputState { get; private set; }

        public SimulationRunningState RunningState { get; private set; }

        public LevelCompleteState LevelCompleteState { get; private set; }

        public IGameState State { get; private set; }

        public IPlayerActorControls PlayerActorControls { get; private set; }

        public GameSettings Settings { get; private set; }

        protected virtual void Awake()
        {
            Settings = _gameSettings.Settings;
            PlayerActorControls = _monkeyTestPlayerSimulationWithRandomInputs
                ? new MonkeyTestRandomInputPlayerActorControls()
                : Player.Controls.PlayerActorControls.Create(Settings);
            GameMenuState = new GameMenuState(this);
            WaitingForSimulationInputState = new WaitingForSimulationInputState(this, Gui.AvailableDirectionsIndication);
            RunningState = new SimulationRunningState(this);
            LevelCompleteState = new LevelCompleteState(this);
        }

        protected void Start()
        {
            State = WaitingForSimulationInputState.Enter();
        }

        protected virtual void FixedUpdate()
        {
            GameStateId originalState = State.Id;
            State = State.FixedUpdate();
            if (State.Id != originalState)
            {
                ApplyIsGameMenuAvailable();
            }
        }

        private void ApplyIsGameMenuAvailable()
        {
            Gui.GameMenu.SetCanOpen = State.Id.GetIsGameMenuAvailable();
        }

        public GridDirection GetInputDirection(GridDirections availableDirections) => PlayerActorControls.GetDirectionChange(availableDirections);

        protected void OnDestroy()
        {
            PlayerActorControls.Destroy();
        }
    }
}