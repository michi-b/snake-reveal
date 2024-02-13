using System;
using System.Diagnostics;
using CustomPropertyDrawers;
using Extensions;
using Game.Grid;
using Game.Gui.AvailableDirectionsIndication;
using Game.Gui.DebugInfo;
using Game.Gui.GameInfo;
using Game.Gui.GameMenu;
using Game.Player;
using Game.Player.Simulation;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
        [SerializeField] private GameMenu _gameMenu;
        [SerializeField] private AvailableDirectionsIndication _availableDirectionsIndication;

        [SerializeField, ToggleLeft] private bool _monkeyTestPlayerSimulationWithRandomInputs;
        [SerializeField, Range(0f, 1f)] private float _targetCoverage = 0.8f;

        private int _startingCellCount;
        private int _coveredCellCount;
        private int _targetCellCount;

        private PlayerSimulation _playerSimulation;

        private int Ticks { get; set; }

        private State CurrentState
        {
            get => _currentState;
            set
            {
                Debug.Assert(value != _currentState);

                _playerSimulation.ControlsEnabled = value != State.Paused;

                LogStateTransition(_currentState, value);

                _currentState = value;
            }
        }

        private State _currentState = State.WaitingForInput;

        protected virtual void Awake()
        {
            _debugInfoGui.TargetCellCount = _targetCellCount = (int)(_targetCoverage * _grid.GetCellCount());
            _debugInfoGui.TotalCellCount = _grid.GetCellCount();

            _playerSimulation = new PlayerSimulation(_grid, _playerActor, _drawnShape, _drawingLineChain, _monkeyTestPlayerSimulationWithRandomInputs);
            _startingCellCount = _coveredCellCount = _playerSimulation.CoveredCellCount;
            UpdatePercentCompletionDisplay();

            _availableDirectionsIndication.transform.SetLocalPositionXY(_playerActor.transform.localPosition);
            _availableDirectionsIndication.SetVisible(true);

#if DEBUG
            _debugInfoGui.SimulationTicks = 0;
            _debugInfoGui.SimulationTime = 0f;
#endif
        }

        protected virtual void FixedUpdate()
        {
            if (HandlePauseState())
            {
                return;
            }

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

            // move player and update drawing
            _playerSimulation.Move();

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

        /// <returns>true if the simulation is paused (or waiting for input) at the moment</returns>
        private bool HandlePauseState()
        {
            bool needsPause = _gameMenu.AnimatorState.IsSomewhatOpen;

            switch (CurrentState)
            {
                case State.Running:
                    if (needsPause)
                    {
                        CurrentState = State.Paused;
                    }

                    return false;
                case State.Paused:
                    if (!needsPause)
                    {
                        CurrentState = State.WaitingForInput;
                    }

                    return true;
                case State.WaitingForInput:
                    if (needsPause)
                    {
                        CurrentState = State.Paused;
                    }

                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdatePercentCompletionDisplay()
        {
            _debugInfoGui.CoveredCellCount = _coveredCellCount;
            _gameInfoGui.PercentCompletion = (_coveredCellCount - _startingCellCount) / (float)_targetCellCount;
        }

        [Conditional("DEBUG")]
        private void LogStateTransition(State stateFrom, State stateTo)
        {
            Debug.Log($"Simulation changes state \"{stateFrom}\" => \"{stateTo}\"");
        }

        private enum State
        {
            Running,
            Paused,
            WaitingForInput
        }
    }
}