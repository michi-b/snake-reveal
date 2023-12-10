using System;
using Game.Enums;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Lines
{
    public class LineLoop : LineContainer
    {
        [SerializeField] private Line _start;
        [SerializeField] private Turn _turn;

        public Line Start => _start;

        public Turn Turn
        {
            get => _turn;
            private set => _turn = value;
        }

        public void Set(params int2[] positions)
        {
#if DEBUG
            Debug.Assert(Start == null);
            Debug.Assert(positions.Length >= 4);
#endif

            Line previous = null;

            for (int index = 0; index < positions.Length; index++)
            {
                Line line = Create(positions[index], positions[(index + 1) % positions.Length]);
                if (previous != null)
                {
                    previous.Next = line;
                    line.Previous = previous;
                }
                else
                {
                    _start = line;
                }

                previous = line;
            }

            Start.Previous = previous;
            // ReSharper disable once PossibleNullReferenceException because this is not possible due to assertion of positions.Length
            previous.Next = Start;

            EvaluateClockRotation();
        }

        private void EvaluateClockRotation()
        {
            int clockwiseWeight = 0;
            Line current = Start;
            do
            {
                Line next = current.Next;
                // ReSharper disable once PossibleNullReferenceException because the line loop is always initialized to be fully connected
                Turn turn = current.Direction.GetTurn(next.Direction);
                clockwiseWeight += turn.GetClockwiseWeight();
                current = next;
            } while (current != Start);

            Debug.Assert(math.abs(clockwiseWeight) == 4);

            Turn = clockwiseWeight > 0 ? Turn.Clockwise : Turn.CounterClockwise;
        }

        public Line FindLineAt(int2 position, Predicate<Line> filter = null)
        {
            if (Start == null)
            {
                throw new InvalidOperationException("Line loop has no start");
            }

            Line current = Start;
            do
            {
                Debug.Assert(current != null);

                if ((filter == null || filter(current)) && current.Contains(position))
                {
                    return current;
                }

                current = current.Next;
                if (current == null)
                {
                    throw new InvalidOperationException("Line loop is not closed");
                }
            } while (current != Start);

            return null;
        }
    }
}