using System;
using Game.Enums;
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
        [SerializeField] private LineLoop _collectedShape;

        [SerializeField] private int2 _startShapeHalfSize = new(5, 5);

        private readonly PlayerMovementMode _playerMovementMode = PlayerMovementMode.ShapeTravel;

        private PlayerActorControls _playerControls;

        // current/last line the player is/was traveling on
        private Line _shapeTravelLine;

        // current/last rotation the player is/was traveling on the collected shape
        private Turn _shapeTravelTurn;

        protected virtual void Awake()
        {
            _playerControls = new PlayerActorControls(RequestDirectionChange);
        }

        protected virtual void Start()
        {
            int2 center = _grid.Size / 2;

            int2 bottomLeft = center - _startShapeHalfSize;
            int2 topRight = center + _startShapeHalfSize;
            var bottomRight = new int2(topRight.x, bottomLeft.y);
            var topLeft = new int2(bottomLeft.x, topRight.y);

            _collectedShape.Set(_grid, _lineCache, topLeft, topRight, bottomRight, bottomLeft);
            Debug.Assert(_collectedShape.Turn == Turn.Clockwise);

            _playerActor.GridPosition = (topLeft + topRight) / 2; // place player at top center of initial shape
            _shapeTravelLine = _collectedShape.FindLineAt(_playerActor.GridPosition);
            _shapeTravelTurn = _collectedShape.Turn;

            Debug.Assert(_shapeTravelLine != null && _shapeTravelLine.Contains(_playerActor.GridPosition));

            _playerActor.Direction = _shapeTravelLine.Direction;
        }

        protected virtual void FixedUpdate()
        {
            for (int moveIndex = 0; moveIndex < _playerActor.Speed; moveIndex++)
            {
                switch (_playerMovementMode)
                {
                    case PlayerMovementMode.ShapeTravel:
                        EvaluateShapeTraveling();
                        break;
                    case PlayerMovementMode.Drawing:
                        MovePlayerDrawing();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected virtual void OnEnable()
        {
            _playerControls.Enable();
        }

        protected virtual void OnDisable()
        {
            _playerControls.Disable();
        }

        private void RequestDirectionChange(GridDirection direction)
        {
        }

        private void EvaluateShapeTraveling()
        {
            int2 playerPosition = _playerActor.GridPosition;

            Debug.Assert(_shapeTravelLine.Contains(playerPosition));

            bool travelTurnIsLoopTurn = _collectedShape.Turn == _shapeTravelTurn;
            bool isAtLineEnd = playerPosition.Equals(travelTurnIsLoopTurn ? _shapeTravelLine.End : _shapeTravelLine.Start);

            // if at line end, switch to next line and adjust direction
            if (isAtLineEnd)
            {
                _shapeTravelLine = travelTurnIsLoopTurn ? _shapeTravelLine.Next : _shapeTravelLine.Previous;
                Debug.Assert(_shapeTravelLine != null, nameof(_shapeTravelLine) + " != null");
                Debug.Assert(_shapeTravelLine.Contains(playerPosition));
                _playerActor.Direction = travelTurnIsLoopTurn ? _shapeTravelLine.Direction : _shapeTravelLine.Direction.GetOpposite();
            }

            _playerActor.GridPosition += _playerActor.Direction.ToInt2();

            if (!_shapeTravelLine.Contains(_playerActor.GridPosition))
            {
                throw new InvalidOperationException(
                    $"Updated player position {_playerActor.GridPosition} is not contained in current shape travel line {_shapeTravelLine.DebuggerDisplay}");
            }

            // todo: apply grid position only once per frame instead (and extrapolated)
            _playerActor.ApplyGridPosition(_grid);
        }

        private void MovePlayerDrawing()
        {
            // if (_playerActor.Direction != GridDirection.None)
            // {
            //     _playerActor.GridPosition += _playerActor.Direction.ToInt2();
            //     _playerActor.GridPosition = _grid.Clamp(_playerActor.GridPosition);
            //
            //     _playerActor.transform.SetLocalPositionXY(_grid.GetScenePosition(_playerActor.GridPosition));
            // }
        }
    }
}