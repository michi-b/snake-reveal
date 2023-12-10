using System;
using System.Diagnostics;
using Extensions;
using Game.Enums;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Lines
{
    [RequireComponent(typeof(LineRenderer))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Line : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private int2 _start;
        [SerializeField] private int2 _end;
        [SerializeField] private Line _next;
        [SerializeField] private Line _previous;
        [SerializeField] private GridDirection _direction;

        private LineRenderer _lineRenderer;

        private LineRenderer LineRenderer => _lineRenderer ? _lineRenderer : _lineRenderer = GetComponent<LineRenderer>();

        // start position in grid space
        public int2 Start
        {
            get => _start;
            private set
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
        public int2 End
        {
            get => _end;
            private set
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
        public Line Next
        {
            get => _next;
            set => _next = value;
        }

        // previous line in the chain, if connected
        [CanBeNull]
        public Line Previous
        {
            get => _previous;
            set => _previous = value;
        }

        public GridDirection Direction => _direction;

        public string DebuggerDisplay => $"{Start}->{End}";

        public void Initialize(SimulationGrid grid)
        {
            _grid = grid;
        }

        public void Place(int2 start, int2 end)
        {
            LineRenderer lineRenderer = LineRenderer;

            Start = start;
            End = end;
            _direction = start.GetDirection(end);

            Debug.Assert(_direction != GridDirection.None);

            LineRenderer.enabled = true;

            Debug.Assert(lineRenderer.positionCount == 2);
        }

        public bool Contains(int2 topCenter)
        {
            return Direction.GetOrientation() switch
            {
                AxisOrientation.Horizontal => Start.y == topCenter.y // same height
                                              && Math.Sign(Start.x - topCenter.x) != Math.Sign(End.x - topCenter.x), // in horizontal range
                AxisOrientation.Vertical => Start.x == topCenter.x // same horizontal position
                                            && Math.Sign(Start.y - topCenter.y) != Math.Sign(End.y - topCenter.y), // in height range
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public GridDirection GetDirection(bool followLineDirection)
        {
            return followLineDirection ? Direction : Direction.GetOpposite();
        }

        public Line GetNext(bool followLineDirection)
        {
            return followLineDirection ? Next : Previous;
        }

        public int2 GetEnd(bool followLineDirection)
        {
            return followLineDirection ? End : Start;
        }

        public bool TryExtend(int2 newEnd)
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
    }
}