using System;
using System.Globalization;
using CustomPropertyDrawers;
using Game.Enums;
using Game.Player;
using Game.Simulation.Grid;
using Game.UI.DebugInfo;
using Game.UI.GameInfo;
using UnityEngine;
using Utility;

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
#if DEBUG
            _debugInfoGui.TargetCellCount = _targetCellCount = (int)(_targetCoverage * _grid.GetCellCount());
            _debugInfoGui.TotalCellCount = _grid.GetCellCount();
#endif

            _playerSimulation = new PlayerSimulation(this, _monkeyTestPlayerSimulationWithRandomInputs);

            _startingCellCount = _coveredCellCount = _playerSimulation.CoveredCellCount;

            UpdatePercentCompletionDisplay();

#if DEBUG
            _debugInfoGui.SimulationTicks = 0;
            _debugInfoGui.SimulationTime = 0f;
#endif
        }

        public virtual SimulationUpdateResult SimulationUpdate()
        {
            if (Ticks == int.MaxValue)
            {
                ApplicationUtility.Quit(ExitCodes.TicksCountOverflow);
                throw new InvalidOperationException("Ticks would overflow after after " +
                                                    (int.MaxValue * Time.fixedDeltaTime / (60 * 60 * 24)).ToString(CultureInfo.InvariantCulture) +
                                                    $"days after game scene load, quitting...");
            }

            Ticks++;

#if DEBUG
            _debugInfoGui.SimulationTicks = Ticks;
            _debugInfoGui.SimulationTime = Ticks * Time.fixedDeltaTime;
#endif

            var result = new SimulationUpdateResult();

            // move player and update drawing
            IPlayerSimulationState oldState = _playerSimulation.CurrentState;
            _playerSimulation.Move(ref result);
            IPlayerSimulationState newState = _playerSimulation.CurrentState;
#if DEBUG
            if (newState != oldState)
            {
                _debugInfoGui.SimulationState = newState.Name;
            }
#endif

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
#if DEBUG
            _debugInfoGui.CoveredCellCount = _coveredCellCount;
#endif
            _gameInfoGui.PercentCompletion = (_coveredCellCount - _startingCellCount) / (float)_targetCellCount;
        }

        public void Resume()
        {
            _playerSimulation.Resume();
        }
    }
}