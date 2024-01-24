using CustomPropertyDrawers;
using Game.Gui.DebugInfo;
using Game.Player;
using Game.Player.Simulation;
using UnityEngine;

namespace Game
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private PlayerActor _playerActor;
        [SerializeField] private DrawnShape _drawnShape;
        [SerializeField] private DrawingChain _drawingLineChain;
        [SerializeField] private DebugInfoGui _debugInfoGui;

        [SerializeField, ToggleLeft] private bool _monkeyTestPlayerSimulationWithRandomInputs;

        private PlayerSimulation _playerSimulation;

        // with a fixed time step of 0.0083, this int will overflow after 206,2976188668982 days
        private int Ticks { get; set; }

        protected virtual void Awake()
        {
            _playerSimulation = new PlayerSimulation(_playerActor, _drawnShape, _drawingLineChain, _monkeyTestPlayerSimulationWithRandomInputs);
        }

        protected virtual void FixedUpdate()
        {
            Ticks++;
#if UNITY_EDITOR
            _debugInfoGui.SimulationTicks = Ticks;
            _debugInfoGui.SimulationTime = Ticks * Time.fixedDeltaTime;
#endif
            _playerSimulation.Move();
        }

        protected virtual void OnEnable()
        {
            _playerSimulation.ControlsEnabled = true;
        }

        protected virtual void OnDisable()
        {
            _playerSimulation.ControlsEnabled = false;
        }
    }
}