using Game.Lines;
using Game.Player;
using UnityEngine;

namespace Game
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private PlayerActor _playerActor;
        [SerializeField] private LineCache _lineCache;
        [SerializeField] private LineLoop _shape;

        private PlayerDrawingSimulation _playerDrawingSimulation;

        protected virtual void Awake()
        {
            _playerDrawingSimulation = new PlayerDrawingSimulation(_grid, _playerActor, _shape);
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