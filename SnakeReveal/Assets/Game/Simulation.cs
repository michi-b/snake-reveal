using CustomPropertyDrawers;
using Game.Grid;
using Game.Gui.DebugInfo;
using Game.Gui.GameInfo;
using Game.Player;
using Game.Player.Simulation;
using UnityEngine;

namespace Game
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private PlayerActor _playerActor;
        [SerializeField] private DrawnShape _drawnShape;
        [SerializeField] private DrawingChain _drawingLineChain;
        [SerializeField] private DebugInfoGui _debugInfoGui;
        [SerializeField] private GameInfoGui _gameInfoGui;

        [SerializeField, ToggleLeft] private bool _monkeyTestPlayerSimulationWithRandomInputs;

        [SerializeField] private int _gridCellCount;
        [SerializeField] private int _coveredCellCount;

        private PlayerSimulation _playerSimulation;

        // with a fixed time step of 0.0083, this int will overflow after 206,2976188668982 days
        private int Ticks { get; set; }

        protected virtual void Awake()
        {
            _playerSimulation = new PlayerSimulation(_playerActor, _drawnShape, _drawingLineChain, _monkeyTestPlayerSimulationWithRandomInputs);
            _gridCellCount = _grid.GetCellCount();
            _coveredCellCount = _playerSimulation.CoveredCellCount;
            UpdatePercentCompletionDisplay();
        }

        protected virtual void FixedUpdate()
        {
            Ticks++;
#if UNITY_EDITOR
            _debugInfoGui.SimulationTicks = Ticks;
            _debugInfoGui.SimulationTime = Ticks * Time.fixedDeltaTime;
#endif
            _playerSimulation.Move();

            int newCoveredCellCount = _playerSimulation.CoveredCellCount;
            if (newCoveredCellCount != _coveredCellCount)
            {
                _coveredCellCount = newCoveredCellCount;
                UpdatePercentCompletionDisplay();
            }
        }

        private void UpdatePercentCompletionDisplay()
        {
            _gameInfoGui.PercentCompletion = _coveredCellCount / (float)_gridCellCount;
        }

        protected virtual void OnEnable()
        {
            _playerSimulation.ControlsEnabled = true;
        }

        protected virtual void OnDisable()
        {
            _playerSimulation.ControlsEnabled = false;
        }
    }
}