using CustomPropertyDrawers;
using Game.Player;
using Game.Player.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private PlayerActor _playerActor;
        [SerializeField] private DrawnShape _drawnShape;
        [SerializeField] private DrawingChain _drawingLineChain;
        [SerializeField] private Text _tickCounter;
        [SerializeField] private Text _timeCounter;
        [SerializeField, ToggleLeft] private bool _monkeyTestPlayerSimulationWithRandomInputs;
        
        private PlayerSimulation _playerSimulation;

        // with a fixed time step of 0.0083, this int will overflow after 206,2976188668982 days
        public int Ticks { get; private set; }

        protected virtual void Awake()
        {
            _playerSimulation = new PlayerSimulation(_playerActor, _drawnShape, _drawingLineChain, _monkeyTestPlayerSimulationWithRandomInputs);
        }

        protected virtual void FixedUpdate()
        {
            Ticks++;
            _tickCounter.text = Ticks.ToString();
            _timeCounter.text = (Ticks * 0.0083).ToString("F2");
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