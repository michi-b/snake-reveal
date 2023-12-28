using System;
using System.Collections;
using System.Collections.Generic;
using Game.Enums;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Lines.Deprecated
{
    public class DeprecatedLineChain : DeprecatedLineContainer, IEnumerable<DeprecatedLine>
    {
        [SerializeField] private DeprecatedLine _start;
        [SerializeField] private DeprecatedLine _end;

        public DeprecatedLine End => _end;

        public DeprecatedLine Start => _start;

        public bool IsCleared => _start == null && _end == null;

        public IEnumerator<DeprecatedLine> GetEnumerator()
        {
            DeprecatedLine current = _start;
            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Set(params Vector2Int[] positions)
        {
            Debug.Assert(IsCleared);
            Debug.Assert(positions.Length >= 2);

            _start = _end = GetLine(positions[0], positions[1]);

            for (int i = 1; i < positions.Length - 1; i++)
            {
                Extend(positions[i + 1]);
            }
        }

        public bool Contains(Vector2Int position, Predicate<DeprecatedLine> filter = null)
        {
            return FindLineAt(position, filter) != null;
        }

        public DeprecatedLine FindLineAt(Vector2Int position, Predicate<DeprecatedLine> filter = null)
        {
            DeprecatedLine current = _start;
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

        public void Extend(Vector2Int position)
        {
            Debug.Assert(_end != null);

            if (!_end.TryExtend(position))
            {
                Append(position);
            }
        }

        public void Clear()
        {
            DeprecatedLine current = _start;
            while (current != null)
            {
                DeprecatedLine next = current.Next;
                Return(current);
                current = next;
            }

            _start = _end = null;
        }

        private void Append(Vector2Int position)
        {
            DeprecatedLine line = GetLine(_end.End, position);
            _end.Next = line;
            line.Previous = _end;
            _end = line;
        }

        public int GetTurnWeight(Turn turn)
        {
            int result = 0;
            DeprecatedLine current = _start;
            while (current.Next != null)
            {
                result += current.GetTurn().GetWeight(turn);
                current = current.Next;
            }

            return result;
        }

        public void Reverse()
        {
            DeprecatedLine current = _start;
            while (current != null)
            {
                DeprecatedLine next = current.Next;
                current.Reverse();
                current = next;
            }

            (_start, _end) = (_end, _start);
        }

        public void Abandon()
        {
            _start = _end = null;
        }

        public bool GetIsConnected()
        {
            if (_start == _end)
            {
                return _start.Previous == null
                       && _end.Previous == null;
            }

            if (Start == null
                || End == null
                || Start.Previous != null
                || End.Next != null)
            {
                return false;
            }

            DeprecatedLine current = _start.Next;

            while (current != _end)
            {
                if (current == null || current.Previous == null || current.Next == null)
                {
                    return false;
                }

                current = current.Next;
            }

            return _end.Previous != null;
        }
    }
}