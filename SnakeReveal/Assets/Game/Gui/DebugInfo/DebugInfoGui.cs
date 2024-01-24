using System;
using TextDisplay.Abstractions;
using UnityEngine;

namespace Game.Gui.DebugInfo
{
    public class DebugInfoGui : MonoBehaviour
    {
        [SerializeField] private IntDisplay _simulationTicks;
        [SerializeField] private FloatDisplay _simulationTime;

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
    }
}