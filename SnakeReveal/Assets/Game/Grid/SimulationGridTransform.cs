using Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Grid
{
    public class SimulationGridTransform : MonoBehaviour
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

        protected virtual void Reset()
        {
            RecordUndo("Reset Grid Transform");
            _grid = SimulationGrid.EditModeFind();
            if (_grid != null)
            {
                Transform thisTransform = transform;
                _position = _grid.Round(thisTransform.position);
            }

            Apply();
        }

        public void Apply()
        {
            _position = Grid.Clamp(Position);
            transform.SetLocalPositionXY(Grid.GetScenePosition(Position));
            _positionChanged?.Invoke(_position);
        }

        public void RecordUndo(string operationName)
        {
            UndoObjectsBuffer[0] = this;
            UndoObjectsBuffer[1] = transform;
            Undo.RecordObjects(UndoObjectsBuffer, operationName);
        }
    }
}