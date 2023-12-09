using System;
using Extensions;
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

        // the lines of the closed shape in clockwise order
        [SerializeField] private ClockwiseLineLoop _shape;

        [SerializeField] private int2 _startShapeHalfSize = new(5, 5);

        private PlayerMovementMode _playerMovementMode = PlayerMovementMode.ShapeTravel;

        // the line the player is currently traveling on if in shape travel mode,
        // otherwise last line he was traveling on before entering the drawing mode
        private Line _shapeTravelLine;

        protected virtual void Start()
        {
            int2 center = _grid.Size / 2;
            int2 bottomLeft = center - _startShapeHalfSize;
            int2 topRight = center + _startShapeHalfSize;
            var bottomRight = new int2(topRight.x, bottomLeft.y);
            var topLeft = new int2(bottomLeft.x, topRight.y);
            _shape.Set(_grid, _lineCache, topLeft, topRight, bottomRight, bottomLeft);

            int2 topCenter = (topLeft + topRight) / 2;
            _playerActor.GridPosition = topCenter;
            _shapeTravelLine = _shape.Start;
        }

        protected virtual void FixedUpdate()
        {
            for (int moveIndex = 0; moveIndex < _playerActor.Speed; moveIndex++)
            {
                switch (_playerMovementMode)
                {
                    case PlayerMovementMode.ShapeTravel:
                        MovePlayerShapeTraveling();
                        break;
                    case PlayerMovementMode.Drawing:
                        MovePlayerDrawing();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }
        }

        private void MovePlayerShapeTraveling()
        {
            throw new NotImplementedException();
        }

        private void MovePlayerDrawing()
        {
            if (_playerActor.Direction != GridDirection.None)
            {
                _playerActor.GridPosition += _playerActor.Direction.ToInt2();
                _playerActor.GridPosition = _grid.Clamp(_playerActor.GridPosition);

                _playerActor.transform.SetLocalPositionXY(_grid.GetScenePosition(_playerActor.GridPosition));
            }
        }
    }
}