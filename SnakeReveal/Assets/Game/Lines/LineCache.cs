using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Game.Lines
{
    //todo: implement actual caching (this is currently just instantiate and destroy)
    public class LineCache : MonoBehaviour
    {
        private const int InitialCapacity = 1000;

        [SerializeField] private Line _linePrefab;

        private readonly Stack<Line> _cache = new(InitialCapacity);

        private int _currentLineIndex = -1;

        public Line LinePrefab => _linePrefab;

        public Line Get()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return Instantiate(_linePrefab);
            }
#endif
            Line result = _cache.Count == 0 ? Instantiate(_linePrefab) : GetCachedLine();
            result.gameObject.name = "Line" + (++_currentLineIndex).ToString(CultureInfo.InvariantCulture);
            return result;
        }

        public void Return(Line line)
        {
            line.gameObject.SetActive(false);
            _cache.Push(line);
        }

        private Line GetCachedLine()
        {
            Line result = _cache.Pop();
            result.gameObject.SetActive(true);
            return result;
        }
    }
}