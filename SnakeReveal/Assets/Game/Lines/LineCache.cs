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

        [SerializeField] private SimulationGrid _grid;

        private readonly Stack<Line> _cache = new(InitialCapacity);

        private int _currentLineIndex = -1;

        public Line LinePrefab => _linePrefab;

        public Line Get()
        {
            Line result = _cache.Count == 0 ? Instantiate() : GetCachedLine();
            result.gameObject.name = "Line" + (++_currentLineIndex).ToString(CultureInfo.InvariantCulture);
            return result;
        }

        private Line Instantiate()
        {
            Line instance = Instantiate(_linePrefab);
            instance.Initialize(_grid);
            return instance;
        }

        public void Return(Line line)
        {
            line.gameObject.SetActive(false);
            _cache.Push(line);
        }

        private Line GetCachedLine()
        {
            Line result = _cache.Pop();
            result.Initialize();
            result.gameObject.SetActive(true);
            return result;
        }
    }
}