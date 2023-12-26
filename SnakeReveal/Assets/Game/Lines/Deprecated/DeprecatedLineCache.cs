using System.Collections.Generic;
using System.Globalization;
using Extensions;
using UnityEngine;

namespace Game.Lines.Deprecated
{
    //todo: implement actual caching (this is currently just instantiate and destroy)
    public class DeprecatedLineCache : MonoBehaviour
    {
        private const int InitialCapacity = 1000;

        [SerializeField] private DeprecatedLine _linePrefab;

        [SerializeField] private SimulationGrid _grid;

        private readonly Stack<DeprecatedLine> _cache = new(InitialCapacity);

        private int _currentLineIndex = -1;

        public DeprecatedLine LinePrefab => _linePrefab;

        public DeprecatedLine Get()
        {
            DeprecatedLine result = _cache.Count == 0 ? Instantiate() : GetCachedLine();
            result.gameObject.name = "Line" + (++_currentLineIndex).ToString(CultureInfo.InvariantCulture);
            return result;
        }

        private DeprecatedLine Instantiate()
        {
            DeprecatedLine instance = Instantiate(_linePrefab);
            instance.Initialize(_grid);
            return instance;
        }

        public void Return(DeprecatedLine line)
        {
            line.Previous = line.Next = null;
            line.gameObject.SetActive(false);
            line.transform.parent = transform;
            line.transform.SetLocalPositionZ(0f);
            _cache.Push(line);
        }

        private DeprecatedLine GetCachedLine()
        {
            DeprecatedLine result = _cache.Pop();
            result.Initialize();
            result.gameObject.SetActive(true);
            return result;
        }
    }
}