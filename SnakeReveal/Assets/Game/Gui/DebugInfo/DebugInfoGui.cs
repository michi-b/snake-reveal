using TextDisplay.Abstractions;
using UnityEngine;

namespace Game.Gui.DebugInfo
{
    public class DebugInfoGui : MonoBehaviour
    {
        [SerializeField] private IntDisplay _simulationTicks;
        [SerializeField] private FloatDisplay _simulationTime;
        [SerializeField] private IntDisplay _totalCellCount;
        [SerializeField] private IntDisplay _coveredCellCount;
        [SerializeField] private FloatDisplay _coveredCellPercentage;
        [SerializeField] private IntDisplay _targetCellCount;

        public void Start()
        {
#if !UNITY_EDITOR
            gameObject.SetActive(false);
#endif
        }

        public float SimulationTime
        {
            set => _simulationTime.Value = value;
        }

        public int SimulationTicks
        {
            set => _simulationTicks.Value = value;
        }

        public int TotalCellCount
        {
            set
            {
                _totalCellCount.Value = value;
                UpdateCoveredCellPercentage();
            }
        }
        
        public int CoveredCellCount
        {
            set
            {
                _coveredCellCount.Value = value;
                UpdateCoveredCellPercentage();
            }
        }
        
        public int TargetCellCount
        {
            set => _targetCellCount.Value = value;
        }

        private void UpdateCoveredCellPercentage()
        {
            _coveredCellPercentage.Value = (float)_coveredCellCount.Value / _totalCellCount.Value;
        }
    }
}