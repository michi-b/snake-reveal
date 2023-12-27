using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace Game.Lines
{
    /// <summary>
    ///     manages cache of lines of ONE type, i.e. one specific line prefab
    /// </summary>
    public class LineCache : MonoBehaviour
    {
        private const int InitialCapacity = 1000;

        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private LineRenderer _prefab;

        private readonly Stack<LineRenderer> _cache = new(InitialCapacity);

        /// <summary>
        ///     access to private prefab for edit mode instantiation,
        ///     should not be required at runtime
        /// </summary>
        public LineRenderer Prefab => _prefab;

        public LineRenderer Get()
        {
            LineRenderer result;
            
            if (_cache.Count == 0)
            {
                result = Instantiate();
            }
            else
            {
                result = GetCachedLine();
                result.gameObject.SetActive(true);
            }

            return result;
        }

        public void Return(LineRenderer line)
        {
            line.gameObject.SetActive(false);
            line.transform.parent = transform;
            line.transform.SetLocalPositionZ(0f);
            _cache.Push(line);
        }

        private LineRenderer GetCachedLine()
        {
            LineRenderer result = _cache.Pop();
            result.gameObject.SetActive(true);
            return result;
        }

        private LineRenderer Instantiate()
        {
            return Instantiate(_prefab);
        }
    }
}