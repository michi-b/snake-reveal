using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Extensions;
using Game.Enums;
using Game.Grid;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Utility;
using Object = UnityEngine.Object;

namespace Game.Lines
{
    /// <summary>
    ///     minimal immutable struct to cache information of line container lines
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(EdgeCollider2D))]
    public partial class Line : MonoBehaviour
    {
        public const string StartPropertyName = nameof(_start);

        public const string EndPropertyName = nameof(_end);

        public const string DirectionPropertyName = nameof(_direction);

        private static readonly Object[] ThreeObjectsUndoBuffer = new Object[3];
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
        public Vector3 EndWorldPosition => _grid.GetScenePosition(_end).ToVector3(transform.position.z);
        public Vector3 StartWorldPosition => transform.position;

        public Line Previous
        {
            get => _previous;
            set => _previous = value;
        }

        public Line Next
        {
            get => _next;
            set => _next = value;
        }

        protected void Reset()
        {
            _grid = SimulationGrid.EditModeFind();
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

        public bool Contains(Vector2Int position)
        {
            return _direction.GetOrientation() switch
            {
                AxisOrientation.Horizontal => ContainsHorizontalNoAssert(position),
                AxisOrientation.Vertical => ContainsVerticallyNoAssert(position),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static bool ContainsHorizontal(Line line, Vector2Int position)
        {
#if DEBUG
            Debug.Assert(line.Orientation == AxisOrientation.Horizontal);
#endif
            return line.ContainsHorizontalNoAssert(position);
        }

        public static bool ContainsVertical(Line line, Vector2Int position)
        {
#if DEBUG
            Debug.Assert(line.Orientation == AxisOrientation.Vertical);
#endif
            return line.ContainsVerticallyNoAssert(position);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ContainsHorizontalNoAssert(Vector2Int position)
        {
            return _start.y == position.y
                   && _start.x < position.x != _end.x < position.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ContainsVerticallyNoAssert(Vector2Int position)
        {
            return _start.x == position.x
                   && _start.y < position.y != _end.y < position.y;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public SkipFirstLineEnumerable SkipFirst()
        {
            return new SkipFirstLineEnumerable(this);
        }

        public void RecordUndoWithNeighbors(string operationName)
        {
            ThreeObjectsUndoBuffer[0] = Previous;
            ThreeObjectsUndoBuffer[1] = this;
            ThreeObjectsUndoBuffer[2] = Next;
            Undo.RegisterCompleteObjectUndo(ThreeObjectsUndoBuffer, operationName);
        }
    }
}