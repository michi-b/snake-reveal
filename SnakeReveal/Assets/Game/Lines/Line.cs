using System;
using System.Diagnostics;
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
        [SerializeField] private int2 _start;
        [SerializeField] private int2 _end;
        [SerializeField] private Line _next;
        [SerializeField] private Line _previous;
        [SerializeField] private GridDirection _direction;

        private LineRenderer _renderer;

        // start position in grid space
        public int2 Start => _start;

        // end position in grid space
        public int2 End => _end;

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

        protected void Awake()
        {
            _renderer = GetComponent<LineRenderer>();
        }

        public void Place(SimulationGrid grid, int2 start, int2 end)
        {
            _start = start;
            _end = end;
            _direction = GridDirectionUtility.GetDirection(start, end);

            _renderer.enabled = true;

            Debug.Assert(_renderer.positionCount == 2);

            _renderer.SetPosition(0, grid.GetScenePosition(start));
            _renderer.SetPosition(1, grid.GetScenePosition(end));
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
    }
}