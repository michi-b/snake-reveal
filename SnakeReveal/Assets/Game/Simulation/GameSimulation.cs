using System;
using System.Globalization;
using CustomPropertyDrawers;
using Game.Enums;
using Game.Gui;
using Game.Player;
using Game.Simulation.Grid;
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
        [SerializeField] private GuiContainer _gui;

        [SerializeField, ToggleLeft] private bool _monkeyTestPlayerSimulationWithRandomInputs;
        [SerializeField, Range(0f, 1f)] private float _targetCoverage = 0.8f;

        private int _startingCellCount;

        public int Ticks { get; private set; }
        public PlayerActor Player => _player;
        public SimulationGrid Grid => _grid;
        public DrawnShape DrawnShape => _drawnShape;
        public DrawingChain Drawing => _drawing;

        public GuiContainer Gui => _gui;

        public PlayerSimulation PlayerSimulation { get; private set; }

        public int TargetCellCount { get; private set; }

        public int CoveredCellCount { get; private set; }

        public GridDirections GetAvailableDirections() => PlayerSimulation.CurrentState.GetAvailableDirections();

        public float GetPercentCompletion() => (CoveredCellCount - _startingCellCount) / (float)TargetCellCount;

        public float GetSimulationTime() => Ticks * Time.fixedDeltaTime;

        protected virtual void Awake()
        {
            TargetCellCount = (int)(_targetCoverage * _grid.GetCellCount());

            PlayerSimulation = new PlayerSimulation(this, _monkeyTestPlayerSimulationWithRandomInputs);

            _startingCellCount = CoveredCellCount = PlayerSimulation.CoveredCellCount;

            UpdatePercentCompletionDisplay();
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

            var result = new SimulationUpdateResult();

            // move player and update drawing
            PlayerSimulation.Move(ref result);

            // automatic physics simulation is disabled in Physics 2D project settings, and is triggered here instead
            // (ATM Physics 2D is what updates the enemies and resolves collisions mainly)
            Physics2D.Simulate(Time.fixedDeltaTime);

            int newCoveredCellCount = PlayerSimulation.CoveredCellCount;
            if (newCoveredCellCount != CoveredCellCount)
            {
                CoveredCellCount = newCoveredCellCount;
                UpdatePercentCompletionDisplay();
            }

            result.LevelComplete = newCoveredCellCount > TargetCellCount;

            return result;
        }

        private void UpdatePercentCompletionDisplay()
        {
            _gui.GameInfo.PercentCompletion = GetPercentCompletion();
        }


        private bool _isRunning;

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    PlayerSimulation.IsRunning = value;
                }
            }
        }

        protected void OnDestroy()
        {
            PlayerSimulation.Dispose();
        }
    }
}