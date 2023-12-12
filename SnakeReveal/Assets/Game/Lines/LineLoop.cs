using System;
using System.Diagnostics;
using Extensions;
using Game.Enums;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Lines
{
    public class LineLoop : LineContainer
    {
        private const int DefaultIsLoopingCheckThreshold = 100;
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

            EvaluateTurn();
        }

        private void EvaluateTurn()
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

#if DEBUG
            Debug.Assert(math.abs(clockwiseWeight) == 4);
#endif

            Turn = clockwiseWeight > 0 ? Turn.Clockwise : Turn.CounterClockwise;
        }

        public bool OutlineContains(int2 position, Predicate<Line> filter = null)
        {
            return FindLineAt(position, filter) != null;
        }

        public Line FindLineAt(int2 position, Predicate<Line> filter = null)
        {
#if DEBUG
            AssertIsLooping();
#endif

            if (Start == null)
            {
                throw new InvalidOperationException("Line loop has no start");
            }

            Line current = Start;
            do
            {
#if DEBUG
                Debug.Assert(current != null);
#endif

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

        public void Incorporate(LineChain chain, Line startLine, Line endLine)
        {
            int2 startPosition = chain.Start.Start;
            int2 endPosition = chain.End.End;
            bool startLineIsEndLine = startLine == endLine;
            if (startLineIsEndLine)
            {
                GridDirection direction = startPosition.GetDirection(endPosition);
#if DEBUG
                Debug.Assert(startLine.Contains(startPosition) && startLine.Contains(endPosition));
                Debug.Assert(direction.GetOrientation() == startLine.Direction.GetOrientation());
#endif
            }

#if DEBUG
            // assert that chain endpoints actually lie on lines
            Debug.Assert(startLine.Contains(startPosition));
            Debug.Assert(endLine.Contains(endPosition));
#endif
            bool followsShapeTurn = GetFollowsTurn(chain, startLine, startPosition, endLine, endPosition);
#if DEBUG
            const string clockwise = "CLOCKWISE";
            const string counterClockwise = "COUNTER-CLOCKWISE";
            string isClockwiseInfo = $@"(which means it is {Turn switch
            { Turn.Clockwise => followsShapeTurn ? clockwise : counterClockwise,
                Turn.CounterClockwise => followsShapeTurn ? counterClockwise : clockwise,
                _ => "none"
            }})";
            Debug.Log(followsShapeTurn ? $"Connection is IN shape turn ({isClockwiseInfo})" : $"Connection is COUNTER shape turn ({isClockwiseInfo})");
#endif
        }

        private bool GetFollowsTurn(LineChain chain, Line startLine, int2 startPosition, Line endLine, int2 endPosition)
        {
#if DEBUG
            AssertIsLooping();
#endif
            int shapeTurnWeight = GetTurnWeight(startLine, endLine, startPosition, endPosition);
            int chainTurnWeight = chain.GetTurnWeight(Turn);
            int relativeTurnWeight = shapeTurnWeight - chainTurnWeight - 2;
#if DEBUG
            Debug.Assert(Math.Abs(relativeTurnWeight) == 4, $"Relative turn weight is {relativeTurnWeight}");
#endif
            return relativeTurnWeight < 0;
        }

        private int GetTurnWeight(Line startLine, Line endLine, int2 startPosition, int2 endPosition)
        {
            if (startLine == endLine)
            {
                return startPosition.GetDirection(endPosition) == startLine.Direction ? 0 : 4;
            }

            int result = 0;
            Line current = startLine;
            while (current != endLine)
            {
#if DEBUG
                Debug.Assert(current.Next != null);
#endif
                result += current.GetTurn().GetWeight(Turn);
                current = current.Next;
            }

            return result;
        }

        [Conditional("DEBUG")]
        private void AssertIsLooping(int threshold = DefaultIsLoopingCheckThreshold)
        {
            int counter = 0;
#if DEBUG
            Line current = Start;
            do
            {
                counter++;
                Debug.Assert(counter < threshold);
                Debug.Assert(current.Next != null);
                current = current.Next;
            } while (current != Start);
#endif
        }
    }
}