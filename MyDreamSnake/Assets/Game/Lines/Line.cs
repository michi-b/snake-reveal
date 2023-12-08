using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Lines
{
    [RequireComponent(typeof(LineRenderer))]
    public class Line : MonoBehaviour
    {
        private LineRenderer _renderer;

        // start position in grid space
        public int2 Start { get; }

        // end position in grid space
        public int2 End { get; }

        // next line in the chain, if connected
        [CanBeNull] public Line Next { get; set; }

        // previous line in the chain, if connected
        [CanBeNull] public Line Previous { get; set; }

        public GridDirection Direction { get; private set; }

        protected void Awake()
        {
            _renderer = GetComponent<LineRenderer>();
        }

        public void Place(SimulationGrid grid, int2 start, int2 end)
        {
            _renderer.enabled = true;
            Debug.Assert(_renderer.positionCount == 2);
            _renderer.SetPosition(0, grid.GetScenePosition(start));
            _renderer.SetPosition(1, grid.GetScenePosition(end));

            bool isHorizontal = start.x == end.x;
#if DEBUG
            bool isVertical = start.y == end.y;
            Debug.Assert(isHorizontal || isVertical);
#endif
            if (isHorizontal)
            {
                Direction = end.y > start.y ? GridDirection.Up : GridDirection.Down;
            }
            else
            {
                Direction = end.x > start.x ? GridDirection.Right : GridDirection.Left;
            }
        }
    }
}