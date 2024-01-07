using Game.Player;
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

        private PlayerActorMovement _playerActorMovement;

        // with a fixed time step of 0.0083, this int will overflow after 206,2976188668982 days
        public int Ticks { get; private set; }

        protected virtual void Awake()
        {
            _playerActorMovement = new PlayerActorMovement(this, _playerActor, _drawnShape, _drawingLineChain);
        }

        protected virtual void FixedUpdate()
        {
            Ticks++;
            _tickCounter.text = Ticks.ToString();
            _timeCounter.text = (Ticks * 0.0083).ToString("F2");
            _playerActorMovement.Move();
        }

        protected virtual void OnEnable()
        {
            _playerActorMovement.ControlsEnabled = true;
        }

        protected virtual void OnDisable()
        {
            _playerActorMovement.ControlsEnabled = false;
        }
    }
}