using System;
using Extensions;
using Game.Enums;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Lines.Deprecated
{
    public class DeprecatedLineLoop : DeprecatedLineContainer
    {
        private const int DefaultIsLoopingCheckThreshold = 100;
        [SerializeField] private DeprecatedLine _start;
        [SerializeField] private Turn _turn;

        public DeprecatedLine Start => _start;

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

            DeprecatedLine previous = null;

            for (int index = 0; index < positions.Length; index++)
            {
                DeprecatedLine line = GetLine(positions[index], positions[(index + 1) % positions.Length]);
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
            DeprecatedLine current = Start;
            do
            {
                DeprecatedLine next = current.Next;
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

        public bool OutlineContains(int2 position, Predicate<DeprecatedLine> filter = null)
        {
            return FindLineAt(position, filter) != null;
        }

        public bool OutlineContains(DeprecatedLine line)
        {
            DeprecatedLine current = Start;
            do
            {
#if DEBUG
                Debug.Assert(current.Next != null);
#endif
                if (current == line)
                {
                    return true;
                }

                current = current.Next;
            } while (current != Start);

            return false;
        }

        public DeprecatedLine FindLineAt(int2 position, Predicate<DeprecatedLine> filter = null)
        {
#if DEBUG
            Debug.Assert(GetIsLooping());
#endif

            if (Start == null)
            {
                throw new InvalidOperationException("Line loop has no start");
            }

            DeprecatedLine current = Start;
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

        /// <returns>whether the connection was made in shape travel direction</returns>
        public bool Incorporate(DeprecatedLineChain chain, DeprecatedLine startLine, DeprecatedLine endLine)
        {
            int2 chainStartPosition = chain.Start.Start;
            int2 chainEndPosition = chain.End.End;
            bool startLineIsEndLine = startLine == endLine;
            if (startLineIsEndLine)
            {
                GridDirection direction = chainStartPosition.GetDirection(chainEndPosition);
#if DEBUG
                Debug.Assert(startLine.Contains(chainStartPosition) && startLine.Contains(chainEndPosition));
                Debug.Assert(direction.GetOrientation() == startLine.Direction.GetOrientation());
#endif
            }

#if DEBUG
            // assert that chain endpoints actually lie on lines
            Debug.Assert(startLine.Contains(chainStartPosition));
            Debug.Assert(endLine.Contains(chainEndPosition));
#endif
            bool followsLoopTurn = GetFollowsTurn(chain, startLine, chainStartPosition, endLine, chainEndPosition);
#if DEBUG
            const string clockwise = "CLOCKWISE";
            const string counterClockwise = "COUNTER-CLOCKWISE";
            string isClockwiseInfo = $@"(which means it is {Turn switch
            { Turn.Clockwise => followsLoopTurn ? clockwise : counterClockwise,
                Turn.CounterClockwise => followsLoopTurn ? counterClockwise : clockwise,
                _ => "none"
            }})";
            Debug.Log(followsLoopTurn ? $"Connection is IN shape turn ({isClockwiseInfo})" : $"Connection is COUNTER shape turn ({isClockwiseInfo})");
#endif

            foreach (DeprecatedLine chainLine in chain)
            {
                Adopt(chainLine);
            }

            // save original chain end line (non-reversed) before eventually reversing it
            // this will be the new star of the loop
            DeprecatedLine originalChainEndLine = chain.End;
            
            if (!followsLoopTurn)
            {
                chain.Reverse();
#if DEBUG
                Debug.Assert(chain.GetIsConnected());
#endif
                (chainStartPosition, chainEndPosition) = (chainEndPosition, chainStartPosition);
                (startLine, endLine) = (endLine, startLine);
            }

            DeprecatedLine chainStartLine = chain.Start;
            DeprecatedLine chainEndLine = chain.End;
            chain.Abandon();

            if (startLineIsEndLine)
            {
#if DEBUG
                Debug.Log("Start line is end line");
                Debug.Assert(startLine.Previous != null, "startLine.Previous != null");
                Debug.Assert(startLine.Next != null, "startLine.Next != null");
#endif
                DeprecatedLine newStartLine = GetLine(startLine.Start, chainStartPosition);
                newStartLine.Previous = startLine.Previous;
                startLine.Previous.Next = newStartLine;
                newStartLine.Next = chainStartLine;
                chainStartLine.Previous = newStartLine;

                startLine.Start = chainEndPosition;
                startLine.Previous = chainEndLine;
                chainEndLine.Next = startLine;
            }
            else
            {
                // remove lines between start and end
                DeprecatedLine current = startLine.Next;
                while (current != endLine)
                {
#if DEBUG
                    Debug.Assert(current != null);
#endif
                    DeprecatedLine next = current.Next;
                    
                    Return(current);
                    current = next;
                }

                startLine.End = chainStartPosition;
                startLine.Next = chainStartLine;
                chainStartLine.Previous = startLine;

                endLine.Start = chainEndPosition;
                endLine.Previous = chainEndLine;
                chainEndLine.Next = endLine;
            }

            // start may have been removed, but chain end line is now in the loop for sure
            // therefore make chain end the new start
            _start = originalChainEndLine;            
            
#if DEBUG
            if (!GetIsLooping())
            {
                Debug.DebugBreak();
            }
#endif

            return followsLoopTurn;
        }

        private bool GetFollowsTurn(DeprecatedLineChain chain, DeprecatedLine startLine, int2 startPosition, DeprecatedLine endLine, int2 endPosition)
        {
#if DEBUG
            Debug.Assert(GetIsLooping());
#endif
            int shapeTurnWeight = GetTurnWeight(startLine, endLine, startPosition, endPosition);
            int chainTurnWeight = chain.GetTurnWeight(Turn);
            int relativeTurnWeight = shapeTurnWeight - chainTurnWeight - 2;
#if DEBUG
            Debug.Assert(Math.Abs(relativeTurnWeight) == 4, $"Relative turn weight is {relativeTurnWeight}");
#endif
            return relativeTurnWeight < 0;
        }

        private int GetTurnWeight(DeprecatedLine startLine, DeprecatedLine endLine, int2 startPosition, int2 endPosition)
        {
            if (startLine == endLine)
            {
                return startPosition.GetDirection(endPosition) == startLine.Direction ? 0 : 4;
            }

            int result = 0;
            DeprecatedLine current = startLine;
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

        private bool GetIsLooping(int threshold = DefaultIsLoopingCheckThreshold)
        {
            int counter = 0;
#if DEBUG
            DeprecatedLine current = Start;
            do
            {
                counter++;
#if DEBUG
                
                if(counter > threshold)
                {
                    Debug.LogError($"Line loop is not looping after {threshold} iterations");
                    return false;
                }
                if(current.Next == null)
                {
                    Debug.LogError("Line loop is not closed");
                    return false;
                }
#endif
                current = current.Next;
            } while (current != Start);

            return true;
#endif
        }
    }
}