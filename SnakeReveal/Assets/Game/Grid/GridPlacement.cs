using System;
using Extensions;
using Game.Enums;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Game.Grid
{
    public class GridPlacement : MonoBehaviour
    {
        public const string PositionPropertyName = nameof(_position);

        private static readonly Object[] UndoObjectsBuffer = new Object[2];
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private Vector2Int _position;
        [SerializeField] private UnityEvent<Vector2Int> _positionChanged;

        public SimulationGrid Grid => _grid;

        public Vector2Int Position
        {
            get => _position;
            set
            {
                if (value != _position)
                {
                    _position = value;
                    Apply();
                }
            }
        }

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            RecordUndo("Reset Grid Transform");
            _grid = SimulationGrid.EditModeFind();
            if (_grid != null)
            {
                Position = _grid.Round(transform.position);
            }
        }
#endif

        public void Apply()
        {
            _position = Grid.Clamp(Position);
            transform.SetLocalPositionXY(Grid.GetScenePosition(Position));
            _positionChanged?.Invoke(_position);
        }

#if UNITY_EDITOR
        public void RecordUndo(string operationName)
        {
            UndoObjectsBuffer[0] = this;
            UndoObjectsBuffer[1] = transform;
            Undo.RecordObjects(UndoObjectsBuffer, operationName);
        }
#endif
        public bool GetCanMoveInGridBounds(GridDirection requestedDirection)
        {
            return requestedDirection switch
            {
                GridDirection.None => false,
                GridDirection.Right => Position.x < _grid.Size.x,
                GridDirection.Up => Position.y < _grid.Size.y,
                GridDirection.Left => Position.x > 0,
                GridDirection.Down => Position.y > 0,
                _ => throw new ArgumentOutOfRangeException(nameof(requestedDirection), requestedDirection, null)
            };
        }
    }
}