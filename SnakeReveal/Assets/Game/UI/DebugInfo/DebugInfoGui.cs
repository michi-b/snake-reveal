using TextDisplay.Abstractions;
using UnityEngine;

namespace Game.UI.DebugInfo
{
    public class DebugInfoGui : MonoBehaviour
    {
        [SerializeField] private TextRenderer _gameState;
        [SerializeField] private TextRenderer _simulationState;
        [SerializeField] private IntDisplay _simulationTicks;
        [SerializeField] private FloatDisplay _simulationTime;
        [SerializeField] private IntDisplay _totalCellCount;
        [SerializeField] private IntDisplay _coveredCellCount;
        [SerializeField] private FloatDisplay _coveredCellPercentage;
        [SerializeField] private IntDisplay _targetCellCount;


        protected virtual void Awake()
        {
#if DEBUG
            gameObject.SetActive(true);
#else
            gameObject.SetActive(false);
#endif
        }

        public string GameState
        {
            set
            {
                AssertIsDebug();
                _gameState.Text = value;
            }
        }

        public string SimulationState
        {
            set
            {
                AssertIsDebug();
                _simulationState.Text = value;
            }
        }

        public float SimulationTime
        {
            set
            {
                AssertIsDebug();
                _simulationTime.Value = value;
            }
        }

        public int SimulationTicks
        {
            set
            {
                AssertIsDebug();
                _simulationTicks.Value = value;
            }
        }

        public int TotalCellCount
        {
            set
            {
                AssertIsDebug();
                _totalCellCount.Value = value;
                UpdateCoveredCellPercentage();
            }
        }

        public int CoveredCellCount
        {
            set
            {
                AssertIsDebug();
                _coveredCellCount.Value = value;
                UpdateCoveredCellPercentage();
            }
        }

        public int TargetCellCount
        {
            set
            {
                AssertIsDebug();
                _targetCellCount.Value = value;
            }
        }

        private void UpdateCoveredCellPercentage()
        {
            _coveredCellPercentage.Value = (float)_coveredCellCount.Value / _totalCellCount.Value;
        }

        private void AssertIsDebug()
        {
#if !DEBUG
            throw new InvalidOperationException("Debug info gui should bot be used in release builds.");
#endif
        }
    }
}