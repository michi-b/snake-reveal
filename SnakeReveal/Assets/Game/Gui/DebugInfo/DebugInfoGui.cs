using Game.Player.Controls;
using Game.Player.Controls.Touch;
using Game.Settings;
using Game.Simulation;
using TextDisplay.Abstractions;
using UnityEngine;
using Utility;

#if !DEBUG
using System; // InvalidOperationException
#endif

namespace Game.Gui.DebugInfo
{
    public class DebugInfoGui : MonoBehaviour, IGameSettingsObserver
    {
        [SerializeField] private Game _game;

        [SerializeField] private TextRenderer _gameState;
        [SerializeField] private TextRenderer _simulationState;
        [SerializeField] private BoolDisplay _simulationIsRunning;
        [SerializeField] private IntDisplay _simulationTicks;
        [SerializeField] private FloatDisplay _simulationTime;
        [SerializeField] private IntDisplay _totalCellCount;
        [SerializeField] private IntDisplay _coveredCellCount;
        [SerializeField] private IntDisplay _targetCellCount;
        [SerializeField] private Vector2Display _dpi;
        [SerializeField] private BoolDisplay _swipesAreEnabled;
        [SerializeField] private BoolDisplay _touch0Active;
        [SerializeField] private Vector2Display _touch0Start;
        [SerializeField] private Vector2Display _swipe0Start;
        [SerializeField] private Vector2Display _touch0Current;

        private GameSettings _gameSettings;

        protected virtual void Start()
        {
            _dpi.Value = ScreenUtility.Dpi;

            _game.Settings.Register(this, true);
        }

        public void OnGameSettingsChanged(GameSettings settings)
        {
            gameObject.SetActive(_game.Settings.DisplayDebugInfo);
        }

        protected virtual void OnDestroy()
        {
            _game.Settings.Deregister(this);
        }

        protected virtual void Update()
        {
            _gameState.Text = _game.State.Name;

            GameSimulation gameSimulation = _game.Simulation;

            _simulationState.Text = gameSimulation.PlayerSimulation.CurrentState.Name;
            _simulationIsRunning.Value = gameSimulation.IsRunning;

            _simulationTicks.Value = gameSimulation.Ticks;
            _simulationTime.Value = gameSimulation.GetSimulationTime();

            _targetCellCount.Value = gameSimulation.TargetCellCount;
            _totalCellCount.Value = gameSimulation.Grid.GetCellCount();
            _coveredCellCount.Value = gameSimulation.CoveredCellCount;

            IPlayerActorControls controls = _game.PlayerActorControls;

            _swipesAreEnabled.Value = controls.IsEnabled;

            if (controls is PlayerActorControls playerActorControls)
            {
                if (playerActorControls.SwipeEvaluation.TryGetTouch(0, out FingerTouch touch))
                {
                    _touch0Start.Value = touch.GetLatestStartScreenPosition();
                    _touch0Active.Value = touch.IsTouching;
                    _touch0Current.Value = touch.GetLatestScreenPosition();
                    _swipe0Start.Value = touch.CurrentSwipeStart;
                }
            }
        }
    }
}