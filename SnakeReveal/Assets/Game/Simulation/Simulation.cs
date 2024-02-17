using CustomPropertyDrawers;
using Game.Enums;
using Game.Grid;
using Game.Gui.DebugInfo;
using Game.Gui.GameInfo;
using Game.Player;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Simulation
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
        [SerializeField, Range(0f, 1f)] private float _targetCoverage = 0.8f;

        private int _startingCellCount;
        private int _coveredCellCount;
        private int _targetCellCount;

        private PlayerSimulation _playerSimulation;

        private int Ticks { get; set; }

        public GridDirections GetAvailableDirections() => _playerSimulation.CurrentState.GetAvailableDirections();

        public PlayerActor PlayerActor => _playerActor;

        protected virtual void Awake()
        {
            _debugInfoGui.TargetCellCount = _targetCellCount = (int)(_targetCoverage * _grid.GetCellCount());
            _debugInfoGui.TotalCellCount = _grid.GetCellCount();

            _playerSimulation = new PlayerSimulation(_grid, PlayerActor, _drawnShape, _drawingLineChain, _monkeyTestPlayerSimulationWithRandomInputs);
            _startingCellCount = _coveredCellCount = _playerSimulation.CoveredCellCount;
            UpdatePercentCompletionDisplay();

#if DEBUG
            _debugInfoGui.SimulationTicks = 0;
            _debugInfoGui.SimulationTime = 0f;
#endif
        }

        public virtual void SimulationUpdate()
        {
            if (Ticks == int.MaxValue)
            {
                Debug.LogError("Ticks would overflow after after 206,2976188668982 days after game scene load, quitting...");
                Application.Quit(ExitCodes.TicksCountOverflow);
                return;
            }

            Ticks++;

#if DEBUG
            _debugInfoGui.SimulationTicks = Ticks;
            _debugInfoGui.SimulationTime = Ticks * Time.fixedDeltaTime;
#endif

            IPlayerSimulationState oldState = _playerSimulation.CurrentState;
            // move player and update drawing
            _playerSimulation.Move();
            IPlayerSimulationState newState = _playerSimulation.CurrentState;
            if(newState != oldState)
            {
                _debugInfoGui.SimulationState = newState.Name;
            }

            // automatic physics simulation is disabled in Physics 2D project settings, and is triggered here instead
            // (ATM Physics 2D is what updates the enemies and resolves collisions mainly)
            Physics2D.Simulate(Time.fixedDeltaTime);

            int newCoveredCellCount = _playerSimulation.CoveredCellCount;
            if (newCoveredCellCount != _coveredCellCount)
            {
                _coveredCellCount = newCoveredCellCount;
                UpdatePercentCompletionDisplay();
            }
        }

        public GridDirection GetRequestedDirection() => _playerSimulation.Controls.GetRequestedDirection();

        private void UpdatePercentCompletionDisplay()
        {
            _debugInfoGui.CoveredCellCount = _coveredCellCount;
            _gameInfoGui.PercentCompletion = (_coveredCellCount - _startingCellCount) / (float)_targetCellCount;
        }
    }
}