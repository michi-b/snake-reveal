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
        private LineRenderer _renderer;

        // start position in grid space
        public int2 Start { get; private set; }

        // end position in grid space
        public int2 End { get; private set; }

        // next line in the chain, if connected
        [CanBeNull] public Line Next { get; set; }

        // previous line in the chain, if connected
        [CanBeNull] public Line Previous { get; set; }

        public GridDirection Direction { get; private set; }

        public string DebuggerDisplay => $"{Start}->{End}";

        protected void Awake()
        {
            _renderer = GetComponent<LineRenderer>();
        }

        public void Place(SimulationGrid grid, int2 start, int2 end)
        {
            Start = start;
            End = end;

            _renderer.enabled = true;

            Debug.Assert(_renderer.positionCount == 2);

            _renderer.SetPosition(0, grid.GetScenePosition(start));
            _renderer.SetPosition(1, grid.GetScenePosition(end));

            Direction = GridDirectionUtility.Evaluate(start, end);
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
    }
}