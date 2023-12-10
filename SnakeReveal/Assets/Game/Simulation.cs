using Game.Lines;
using Game.Player;
using UnityEngine;

namespace Game
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private PlayerActor _playerActor;
        [SerializeField] private LineLoop _shape;
        [SerializeField] private LineChain _lineChain;


        private PlayerDrawingSimulation _playerDrawingSimulation;

        protected virtual void Awake()
        {
            _playerDrawingSimulation = new PlayerDrawingSimulation(_playerActor, _shape, _lineChain);
        }

        protected virtual void FixedUpdate()
        {
            _playerDrawingSimulation.Update();
        }

        protected virtual void OnEnable()
        {
            _playerDrawingSimulation.ControlsEnabled = true;
        }

        protected virtual void OnDisable()
        {
            _playerDrawingSimulation.ControlsEnabled = false;
        }
    }
}