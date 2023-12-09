using Game.Lines;
using Game.Player;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private PlayerActor _playerActor;
        [SerializeField] private LineCache _lineCache;

        // the silhouette lines of the collected closed shape
        [SerializeField] private LineLoop _shape;

        [SerializeField] private int2 _startShapeHalfSize = new(5, 5);

        private PlayerDrawingSimulation _playerDrawingSimulation;

        protected virtual void Awake()
        {
            // int2 center = _grid.Size / 2;
            //
            // int2 bottomLeft = center - _startShapeHalfSize;
            // int2 topRight = center + _startShapeHalfSize;
            // var bottomRight = new int2(topRight.x, bottomLeft.y);
            // var topLeft = new int2(bottomLeft.x, topRight.y);
            //
            // _shape.Set(_grid, _lineCache, topLeft, topRight, bottomRight, bottomLeft);
            // Debug.Assert(_shape.Turn == Turn.Clockwise);
            //
            // _playerActor.Position = (topLeft + topRight) / 2; // place player at top center of initial shape
            // _playerActor.Direction = GridDirection.Right;
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