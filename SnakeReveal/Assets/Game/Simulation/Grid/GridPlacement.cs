using Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Simulation.Grid
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
    }
}