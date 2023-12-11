using System;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Lines
{
    public class LineChain : LineContainer
    {
        [SerializeField] private Line _start;
        [SerializeField] private Line _end;

        public Line End => _end;

        public Line Start => _start;

        public bool IsCleared => _start == null && _end == null;

        public void Set(params int2[] positions)
        {
            Debug.Assert(IsCleared);
            Debug.Assert(positions.Length >= 2);

            _start = _end = Create(positions[0], positions[1]);

            for (int i = 1; i < positions.Length - 1; i++)
            {
                Extend(positions[i + 1]);
            }
        }

        public bool Contains(int2 position, Predicate<Line> filter = null)
        {
            return FindLineAt(position, filter) != null;
        }

        public Line FindLineAt(int2 position, Predicate<Line> filter = null)
        {
            Line current = _start;
            while (current != null)
            {
                if ((filter == null || filter(current)) && current.Contains(position))
                {
                    return current;
                }

                current = current.Next;
            }

            return null;
        }

        public void Extend(int2 position)
        {
            Debug.Assert(_end != null);

            if (!_end.TryExtend(position))
            {
                Append(position);
            }
        }

        public void Clear()
        {
            Line current = _start;
            while (current != null)
            {
                Line next = current.Next;
                Return(current);
                current = next;
            }

            _start = _end = null;
        }

        private void Append(int2 position)
        {
            Line line = Create(_end.End, position);
            _end.Next = line;
            line.Previous = _end;
            _end = line;
        }
    }
}