using System.Collections.Generic;
using Extensions;
using Game.Enums;
using Game.Grid;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Utility;

namespace Game.Lines
{
    /// <summary>
    ///     minimal immutable struct to cache information of line container lines
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(EdgeCollider2D))]
    public partial class Line : MonoBehaviour
    {
        private static readonly List<Vector2> ColliderPointsUpdateBuffer = new() { Vector2.zero, Vector2.right };

        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private LineRenderer _renderer;
        [SerializeField] private EdgeCollider2D _collider;

        [SerializeField] private Vector2Int _start;
        [SerializeField] private Vector2Int _end;
        [SerializeField] private GridDirection _direction;
        [SerializeField] [CanBeNull] private Line _previous;
        [SerializeField] [CanBeNull] private Line _next;

        public Line(Vector2Int start, Vector2Int end)
        {
            _start = start;
            _end = end;
            _direction = _start.GetDirection(_end);
        }

        public SimulationGrid Grid
        {
            set => _grid = value;
        }

        public GridDirection Direction => _direction;

        public Vector2Int Start
        {
            get => _start;
            set
            {
                _start = value;
                if (_previous != null)
                {
                    _previous._end = value;
                    _previous.ApplyPositions();
                }

                ApplyPositions();
            }
        }

        public Vector2Int End
        {
            get => _end;
            set
            {
                _end = value;
                if (_next != null)
                {
                    _next._start = value;
                    _next.ApplyPositions();
                }

                ApplyPositions();
            }
        }

        public AxisOrientation Orientation => _direction.GetOrientation();

        public Vector3 StartWorldPosition => transform.position;
        public Vector3 EndWorldPosition => transform.position + _renderer.GetPosition(1);

        [CanBeNull]
        public Line Previous
        {
            get => _previous;
            set => _previous = value;
        }

        [CanBeNull]
        public Line Next
        {
            get => _next;
            set => _next = value;
        }

        public Vector2Int Delta => _end - _start;

        public Vector2 ScenePositionDelta => _renderer.GetPosition(1);

        protected void Reset()
        {
            _grid = SimulationGrid.EditModeFind();
        }

        protected void OnDrawGizmosSelected()
        {
            if (Selection.Contains(gameObject))
            {
                Color originalGizmoColor = Gizmos.color;
                Gizmos.color = Color.black;

                float diameter = _grid.SceneCellSize.magnitude * 0.25f;
                Vector3 startWorldPosition = StartWorldPosition;
                Vector3 endWorldPosition = EndWorldPosition;
                Gizmos.DrawWireSphere(startWorldPosition, diameter);
                Vector3 direction = endWorldPosition - startWorldPosition;
                GizmosUtility.DrawArrowHead(endWorldPosition, direction, diameter);

                Gizmos.color = originalGizmoColor;
            }
        }

        private void ApplyPositions()
        {
            transform.SetLocalPositionXY(_grid.GetScenePosition(_start));

            Vector2Int delta = _end - _start;
            _direction = _start.GetDirection(_end);

            Vector2 sceneDelta = delta * _grid.SceneCellSize;

            // first point of renderer must always be Vector2.zero
            _renderer.SetPosition(1, sceneDelta);

            // first element in collider points update buffer must always be Vector2.zero
            ColliderPointsUpdateBuffer[1] = sceneDelta;
            _collider.SetPoints(ColliderPointsUpdateBuffer);
        }
    }
}