using System;
using Extensions;
using Game.Enums;
using UnityEngine;

namespace Game.Lines.Colliders
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LineCollider : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private LineContainer _container;
        [SerializeField] private int _index = -1;
        [SerializeField] private Line _line;

        protected void Reset()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        public void Set(LineContainer container, int index)
        {
            _container = container;
            _line = container[index];
            _index = index;
            UpdatePlacement(container.Grid);
        }

        public void UpdatePlacement(SimulationGrid grid)
        {
            transform.position = _line.Start.GetScenePosition(grid).ToVector3(0f);
            switch (_line.Direction.GetOrientation())
            {
                case AxisOrientation.Horizontal:
                    UpdateHorizontalColliderPlacement(grid);
                    break;
                case AxisOrientation.Vertical:
                    UpdateVerticalColliderPlacement(grid);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateHorizontalColliderPlacement(SimulationGrid grid)
        {
            float startToEndX = (_line.End.x - _line.Start.x) * grid.SceneCellSize.x;
            _collider.offset = new Vector2(startToEndX * 0.5f, 0f);
            float length = Mathf.Abs(startToEndX) + grid.SceneCellSize.x;
            _collider.size = new Vector2(length, grid.SceneCellSize.y);
        }

        private void UpdateVerticalColliderPlacement(SimulationGrid grid)
        {
            float startToEndY = (_line.End.y - _line.Start.y) * grid.SceneCellSize.y;
            _collider.offset = new Vector2(0f, startToEndY * 0.5f);
            float length = Mathf.Abs(startToEndY) + grid.SceneCellSize.y;
            _collider.size = new Vector2(grid.SceneCellSize.x, length);
        }
    }
}