using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Game.Lines
{
    //todo: implement actual caching (this is currently just instantiate and destroy)
    public class LineCache : MonoBehaviour
    {
        [SerializeField] private Line _linePrefab;

        private Stack<Line> _cache;

        private int _currentLineIndex = -1;

        public Line Get()
        {
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