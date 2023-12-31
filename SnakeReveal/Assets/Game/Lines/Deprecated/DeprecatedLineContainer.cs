using Extensions;
using Game.Grid;
using UnityEngine;

namespace Game.Lines.Deprecated
{
    public abstract class DeprecatedLineContainer : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private DeprecatedLineCache _lineCache;

        public SimulationGrid Grid => _grid;

        protected void Return(DeprecatedLine line)
        {
            _lineCache.Return(line);
        }

        protected DeprecatedLine GetLine(Vector2Int start, Vector2Int end)
        {
            DeprecatedLine line = _lineCache.Get();
            line.Place(start, end);
            Adopt(line);
            return line;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        protected void Adopt(DeprecatedLine line)
        {
            line.transform.parent = transform;
            line.transform.SetLocalPositionZ(0f);
        }
    }
}