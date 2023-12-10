using Unity.Mathematics;
using UnityEngine;

namespace Game.Lines
{
    public class LineChain : LineContainer
    {
        [SerializeField] private Line _first;
        [SerializeField] private Line _last;

        public Line Last => _last;

        public Line First => _first;

        public void Set(params int2[] positions)
        {
            Debug.Assert(_first == null);
            Debug.Assert(_last == null);
            Debug.Assert(positions.Length >= 2);

            _first = _last = Create(positions[0], positions[1]);

            for (int i = 1; i < positions.Length - 1; i++)
            {
                Extend(positions[i + 1]);
            }
        }

        public void Extend(int2 position)
        {
            Debug.Assert(_last != null);

            if (!_last.TryExtend(position))
            {
                Append(position);
            }
        }

        private void Append(int2 position)
        {
            Line line = Create(_last.End, position);
            _last.Next = line;
            line.Previous = _last;
            _last = line;
        }
    }
}