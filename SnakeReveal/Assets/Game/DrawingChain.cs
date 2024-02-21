using System.Diagnostics;
using Game.Enums;
using Game.Lines;
using Game.Simulation.Grid;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game
{
    public class DrawingChain : MonoBehaviour
    {
        [SerializeField] private LineChain _lineChain;
        public bool IsActive { get; private set; }

        public LineChain Lines => _lineChain;

        public Vector2Int StartPosition
        {
            get
            {
                AssertIsActive();
                return _lineChain.Start.Start;
            }
        }

        public GridDirection StartDirection => _lineChain.Start.Direction;
        public SimulationGrid Grid => _lineChain.Grid;
        public Line LastLine => _lineChain.End;

        public void Activate(Vector2Int start, Vector2Int end)
        {
            _lineChain.Set(new LineData(start, end));
            IsActive = true;
        }

        public void Deactivate()
        {
            _lineChain.Clear();
            IsActive = false;
        }

        public bool Contains(Vector2Int position)
        {
            AssertIsActive();
            return _lineChain.ContainsLineAt(position);
        }

        public void Extend(Vector2Int actorPosition, out bool turned)
        {
            AssertIsActive();
            _lineChain.Extend(actorPosition, out turned);
        }

        [Conditional("DEBUG")]
        private void AssertIsActive()
        {
            Debug.Assert(IsActive);
        }
    }
}