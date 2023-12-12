using Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Lines
{
    public abstract class LineContainer : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private LineCache _lineCache;

        public SimulationGrid Grid => _grid;

        protected void Return(Line line)
        {
            _lineCache.Return(line);
        }

        protected Line GetLine(int2 start, int2 end)
        {
            Line line = _lineCache.Get();
            line.Place(start, end);
            Adopt(line);
            return line;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        protected void Adopt(Line line)
        {
            line.transform.parent = transform;
            line.transform.SetLocalPositionZ(0f);
        }
    }
}