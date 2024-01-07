using System.Diagnostics;
using Game.Lines;
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

        public void Deactivate()
        {
            _lineChain.Clear();
            IsActive = false;
        }
        
        public void Activate(Vector2Int start, Vector2Int end)
        {
            _lineChain.Set(new LineData(start, end));
            IsActive = true;
        }
        
        public bool Contains(Vector2Int position)
        {
            AssertIsActive();
            return _lineChain.ContainsLineAt(position);
        }
        
        public void Extend(Vector2Int actorPosition)
        {
            AssertIsActive();
            _lineChain.Extend(actorPosition);
        }

        [Conditional("DEBUG")]
        private void AssertIsActive()
        {
            Debug.Assert(IsActive);
        }
    }
}
