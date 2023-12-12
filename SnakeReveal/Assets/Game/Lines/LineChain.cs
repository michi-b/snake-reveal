using System;
using System.Collections;
using System.Collections.Generic;
using Game.Enums;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Lines
{
    public class LineChain : LineContainer, IEnumerable<Line>
    {
        [SerializeField] private Line _start;
        [SerializeField] private Line _end;

        public Line End => _end;

        public Line Start => _start;

        public bool IsCleared => _start == null && _end == null;

        public IEnumerator<Line> GetEnumerator()
        {
            Line current = _start;
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

        public void Set(params int2[] positions)
        {
            Debug.Assert(IsCleared);
            Debug.Assert(positions.Length >= 2);

            _start = _end = GetLine(positions[0], positions[1]);

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
            Line line = GetLine(_end.End, position);
            _end.Next = line;
            line.Previous = _end;
            _end = line;
        }

        public int GetTurnWeight(Turn turn)
        {
            int result = 0;
            Line current = _start;
            while (current.Next != null)
            {
                result += current.GetTurn().GetWeight(turn);
                current = current.Next;
            }

            return result;
        }

        public void Reverse()
        {
            Line current = _start;
            while (current != null)
            {
                Line next = current.Next;
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

            Line current = _start.Next;

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