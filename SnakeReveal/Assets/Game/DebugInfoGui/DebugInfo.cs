using TextDisplay.Abstractions;
using UnityEngine;

namespace Game.DebugInfoGui
{
    public class DebugInfo : MonoBehaviour
    {
        [SerializeField] private IntDisplay _simulationTicks;
        [SerializeField] private FloatDisplay _simulationTime;

        public float SimulationTime
        {
            set => _simulationTime.Value = value;
        }

        public int SimulationTicks
        {
            set => _simulationTicks.Value = value;
        }
    }
}