using System.Collections.Generic;
using Extensions;
using Game.Grid;
using UnityEngine;

namespace Game.Lines.Colliders
{
    [RequireComponent(typeof(EdgeCollider2D))]
    public class LineCollider : MonoBehaviour
    {
        private static readonly List<Vector2> SetPointsBuffer = new(2) { Vector2.zero, Vector2.right };
        [SerializeField] private EdgeCollider2D _collider;
        [SerializeField] private LineContainer _container;
        [SerializeField] private int _index = -1;

        protected void Reset()
        {
            _collider = GetComponent<EdgeCollider2D>();
        }

        public void Set(LineContainer container, int index)
        {
            _container = container;
            _index = index;
            UpdatePlacement();
        }

        public void UpdatePlacement()
        {
            Line line = _container[_index];
            SimulationGrid grid = _container.Grid;
            transform.SetLocalPositionXY(grid.GetScenePosition(line.Start));
            SetPointsBuffer[1] = (line.End - line.Start) * grid.SceneCellSize;
            _collider.SetPoints(SetPointsBuffer);
        }
    }
}