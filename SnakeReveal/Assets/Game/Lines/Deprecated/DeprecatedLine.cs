using System;
using System.Diagnostics;
using Game.Enums;
using Game.Grid;
using JetBrains.Annotations;
using UnityEngine;
using Utility;
using Debug = UnityEngine.Debug;

namespace Game.Lines.Deprecated
{
    [RequireComponent(typeof(LineRenderer)), DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class DeprecatedLine : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private Vector2Int _start;
        [SerializeField] private Vector2Int _end;
        [SerializeField] private DeprecatedLine _next;
        [SerializeField] private DeprecatedLine _previous;
        [SerializeField] private GridDirection _direction;

        private LineRenderer _lineRenderer;

        private LineRenderer LineRenderer => _lineRenderer ? _lineRenderer : _lineRenderer = GetComponent<LineRenderer>();


        // start position in grid space

        public Vector2Int Start
        {
            get => _start;
            set
            {
                if (value.Equals(_start))
                {
                    return;
                }

                _start = value;
                LineRenderer.SetPosition(0, _grid.GetScenePosition(_start));
            }
        }


        // end position in grid space

        public Vector2Int End
        {
            get => _end;
            set
            {
                if (value.Equals(_end))
                {
                    return;
                }

                _end = value;
                LineRenderer.SetPosition(1, _grid.GetScenePosition(_end));
            }
        }


        // next line in the chain, if connected

        [CanBeNull]
        public DeprecatedLine Next
        {
            get => _next;
            set => _next = value;
        }


        // previous line in the chain, if connected

        [CanBeNull]
        public DeprecatedLine Previous
        {
            get => _previous;
            set => _previous = value;
        }

        public GridDirection Direction => _direction;

        public string DebuggerDisplay => $"{Start}->{End}";

        public void Place(Vector2Int start, Vector2Int end)
        {
            LineRenderer lineRenderer = LineRenderer;

            Start = start;
            End = end;
            _direction = start.GetDirection(end);

            Debug.Assert(_direction != GridDirection.None);

            LineRenderer.enabled = true;

            Debug.Assert(lineRenderer.positionCount == 2);
        }

        public void Initialize(SimulationGrid grid)
        {
            _grid = grid;
            Initialize();
        }

        public void Initialize()
        {
            _start = new Vector2Int();
            _end = new Vector2Int();
            _next = null;
            _previous = null;
            _direction = GridDirection.None;
        }

        public bool Contains(Vector2Int position)
        {
            return Direction.GetOrientation() switch
            {
                AxisOrientation.Horizontal => Start.y == position.y // same height
                                              && Math.Sign(Start.x - position.x) != Math.Sign(End.x - position.x), // in horizontal range
                AxisOrientation.Vertical => Start.x == position.x // same horizontal position
                                            && Math.Sign(Start.y - position.y) != Math.Sign(End.y - position.y), // in height range
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public GridDirection GetDirection(bool followLineDirection)
        {
            return followLineDirection ? Direction : Direction.GetOpposite();
        }

        public GridDirection GetDirection(Vector2Int position)
        {
            return End.GetDirection(position);
        }

        public Turn GetTurn()
        {
#if DEBUG
            Debug.Assert(Next != null);
            Debug.Assert(Next.Start.Equals(End));
#endif
            return Direction.GetTurn(Next.Direction);
        }

        public DeprecatedLine GetNext(bool followLineDirection)
        {
            return followLineDirection ? Next : Previous;
        }

        public Vector2Int GetEnd(bool followLineDirection)
        {
            return followLineDirection ? End : Start;
        }

        public bool TryExtend(Vector2Int newEnd)
        {
            if (Contains(newEnd))
            {
                return true;
            }

            if (_end.GetDirection(newEnd) != Direction)
            {
                return false;
            }

            End = newEnd;
            return true;
        }

        public void Reverse()
        {
            (Start, End) = (End, Start);
            (Previous, Next) = (Next, Previous);
            _direction = _direction.GetOpposite();
        }
    }
}