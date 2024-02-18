using System;
using System.Globalization;
using CustomPropertyDrawers;
using Game.Enums;
using Game.Grid;
using Game.Gui.DebugInfo;
using Game.Gui.GameInfo;
using Game.Player;
using UnityEngine;

namespace Game.Simulation
{
    public class GameSimulation : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private PlayerActor _player;
        [SerializeField] private DrawnShape _drawnShape;
        [SerializeField] private DrawingChain _drawing;
        [SerializeField] private DebugInfoGui _debugInfoGui;
        [SerializeField] private GameInfoGui _gameInfoGui;

        [SerializeField, ToggleLeft] private bool _monkeyTestPlayerSimulationWithRandomInputs;
        [SerializeField, Range(0f, 1f)] private float _targetCoverage = 0.8f;

        private int _startingCellCount;
        private int _coveredCellCount;
        private int _targetCellCount;

        private PlayerSimulation _playerSimulation;

        private int Ticks { get; set; }
        public PlayerActor Player => _player;
        public SimulationGrid Grid => _grid;
        public DrawnShape DrawnShape => _drawnShape;
        public DrawingChain Drawing => _drawing;

        public GridDirections GetAvailableDirections() => _playerSimulation.CurrentState.GetAvailableDirections();

        protected virtual void Awake()
        {
            _debugInfoGui.TargetCellCount = _targetCellCount = (int)(_targetCoverage * _grid.GetCellCount());
            _debugInfoGui.TotalCellCount = _grid.GetCellCount();

            _playerSimulation = new PlayerSimulation(this, _monkeyTestPlayerSimulationWithRandomInputs);

            _startingCellCount = _coveredCellCount = _playerSimulation.CoveredCellCount;

            UpdatePercentCompletionDisplay();

            _debugInfoGui.SimulationTicks = 0;
            _debugInfoGui.SimulationTime = 0f;
        }

        public virtual SimulationUpdateResult SimulationUpdate()
        {
            if (Ticks == int.MaxValue)
            {
                Application.Quit(ExitCodes.TicksCountOverflow);
                throw new InvalidOperationException("Ticks would overflow after after " +
                                                    (int.MaxValue * Time.fixedDeltaTime / (60 * 60 * 24)).ToString(CultureInfo.InvariantCulture) +
                                                    $"days after game scene load, quitting...");
            }

            Ticks++;

            _debugInfoGui.SimulationTicks = Ticks;
            _debugInfoGui.SimulationTime = Ticks * Time.fixedDeltaTime;

            SimulationUpdateResult result = new SimulationUpdateResult();
            
            // move player and update drawing
            IPlayerSimulationState oldState = _playerSimulation.CurrentState;
            _playerSimulation.Move(ref result);
            IPlayerSimulationState newState = _playerSimulation.CurrentState;
            if (newState != oldState)
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

            return result;
        }

        public GridDirection GetRequestedDirection() => _playerSimulation.Controls.GetRequestedDirection();

        private void UpdatePercentCompletionDisplay()
        {
            _debugInfoGui.CoveredCellCount = _coveredCellCount;
            _gameInfoGui.PercentCompletion = (_coveredCellCount - _startingCellCount) / (float)_targetCellCount;
        }
    }
}