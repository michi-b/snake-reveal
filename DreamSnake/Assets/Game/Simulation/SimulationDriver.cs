using System;
using UnityEngine;

namespace Game.Simulation
{
    [DefaultExecutionOrder(-1)]
    public class SimulationDriver : MonoBehaviour
    {
        [SerializeField] private SimulationStepService _simulationStepService;
        [SerializeField] private readonly double _simulationStepDuration = 0.0083333333333333332; // 120 fps
        [SerializeField] private int _maxSimulationStepsPerFrame = 10; // 60 fps

        private int _lastDisableSimulationStepCount;

        // the time since level load when the simulation was last paused
        private double _lastDisableTime;

        // the time since level load when the simulation was last played/resumed
        private double _lastEnableTime;

        public int SimulationStepCount { get; private set; }
        public double SimulationTime { get; private set; }

        protected virtual void Update()
        {
            double timeSinceLastEnable = Time.timeSinceLevelLoadAsDouble - _lastEnableTime;
            int targetSimulationStepsSinceLastEnable = (int)(timeSinceLastEnable / _simulationStepDuration);
            int targetSimulationStepCount = _lastDisableSimulationStepCount + targetSimulationStepsSinceLastEnable;
            int targetSimulationStepsToRunThisFrame = targetSimulationStepCount - SimulationStepCount;

            int simulationStepsToRunThisFrame = Math.Min(targetSimulationStepsToRunThisFrame, _maxSimulationStepsPerFrame);

            if (simulationStepsToRunThisFrame < targetSimulationStepsToRunThisFrame)
            {
                Debug.LogWarning
                (
                    $"Would need to run {targetSimulationStepsToRunThisFrame} in this frame to make the simulation catch up,"
                    + $" but the maximum number of simulation steps per frame is capped at {_maxSimulationStepsPerFrame}"
                );
            }

            for (int i = 0; i < simulationStepsToRunThisFrame; i++)
            {
                SimulationStepCount++;
                SimulationTime = _simulationStepDuration * SimulationStepCount;
                ExecuteSimulationStep();
            }
        }

        protected virtual void OnEnable()
        {
            _lastEnableTime = Time.timeSinceLevelLoadAsDouble;
            _simulationStepService.RegisterDriver(this);
        }

        protected virtual void OnDisable()
        {
            _lastDisableTime = Time.timeSinceLevelLoadAsDouble;
            _lastDisableSimulationStepCount = SimulationStepCount;
            _simulationStepService.DeregisterDriver(this);
        }

        private void ExecuteSimulationStep()
        {
            _simulationStepService.ExecuteSimulationStep();
        }
    }
}