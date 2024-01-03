namespace Game.Lines
{
    public partial class LineContainer
    {
#if false
        public static class InsertUtility
        {
            public static void Insert(DoubleLinkedLineList loop, DoubleLinkedLineList chain)
            {
#if DEBUG
                Debug.Assert(loop.Loop, "Line chain must be a loop to insert another chain");
                Debug.Assert(!chain.Loop, "Inserted line chain must not be a loop");
                Debug.Assert(Math.Abs(loop._clockwiseTurnWeight) == 4, "A looping line chain must have a clockwise turn weight of 4 or -4 " +
                                                                       "(full single turnaround in either clockwise or counter clockwise direction)");
#endif
                Turn loopTurn = loop.Turn;

                // Get line indices of breakout position and re-insertion position.
                // Note that the lines of the loop are filtered by their direction.
                // This is not only a performance optimization, but also ensures that we get the correct perpendicular lines at the correct positions.
                GridDirection insertionStartLineDirection = chain[0].Direction.Turn(loopTurn);
                GridDirection insertionEndLineDirection = chain[^1].Direction.Turn(loopTurn.Reverse());

                Vector2Int chainStart = chain[0].Start;
                if (!TryGetIndexAt(loop, chainStart, insertionStartLineDirection, out int insertionStartIndex))
                {
                    throw GetPositionIsNotOnLoopException("start", chainStart, insertionStartLineDirection);
                }

                Vector2Int chainEnd = chain[^1].End;
                if (!TryGetIndexAt(loop, chainEnd, insertionEndLineDirection, out int insertionEndIndex))
                {
                    throw GetPositionIsNotOnLoopException("end", chainEnd, insertionEndLineDirection);
                }

                int loopTurnWeight = GetTurnWeight(loop, insertionStartIndex, insertionEndIndex);
                int chainTurnWeight = loop._clockwiseTurnWeight == 4 ? chain._clockwiseTurnWeight : -chain._clockwiseTurnWeight;
                // Turn weight delta is always one of -6, -2, 2, and 6.
                // -2 and 2 being the regular cases, -6 and 6 might occur when chain start and end are the same,
                // or when the loop contains the wraparound point (start of line 0) between chain start and end.
                // In any case, checking whether the delta sign seems to be a trustworthy way to determine whether the connection is made in loop turn or countering it.
                int deltaTurnWeight = chainTurnWeight - loopTurnWeight;

#if UNITY_EDITOR
                Debug.Log($"\"{(deltaTurnWeight > 0 ? "IN TURN" : "NOT IN TURN")}\" with DeltaTurnWeight = \"{deltaTurnWeight}\"\n" +
                          $"ChainTurnWeight = \"{chainTurnWeight}\"-LoopTurnWeight = \"{loopTurnWeight}\"\t (index \"{insertionStartIndex}\" -> index {insertionEndIndex}\")");
#endif
            }

            private static bool TryGetIndexAt(DoubleLinkedLineList loop, Vector2Int position, GridDirection lineDirection, out int index)
            {
                Func<Line, Vector2Int, bool> lineContainsCheck = lineDirection.GetOrientation() switch
                {
                    AxisOrientation.Horizontal => Line.ContainsHorizontal,
                    AxisOrientation.Vertical => Line.ContainsVertical,
                    _ => throw new ArgumentOutOfRangeException()
                };

                for (int i = 0; i < loop._lines.Count; i++)
                {
                    Line line = loop._lines[i];
                    if (line.Direction != lineDirection)
                    {
                        continue;
                    }

                    if (lineContainsCheck(line, position))
                    {
                        index = i;
                        return true;
                    }
                }

                index = -1;
                return false;
            }

            private static int GetTurnWeight(DoubleLinkedLineList loop, int startIndex, int endIndex)
            {
                int clockwiseTurnWeight = 0;
                int currentIndex = startIndex;

                // note: not using < but != because the index might loop around
                while (currentIndex != endIndex)
                {
                    int nextIndex = (currentIndex + 1) % loop._lines.Count;
                    GridDirection currentDirection = loop._lines[currentIndex].Direction;
                    GridDirection nextDirection = loop._lines[nextIndex].Direction;
                    clockwiseTurnWeight += currentDirection.GetTurn(nextDirection).GetClockwiseWeight();
                    currentIndex = nextIndex;
                }

                return loop._clockwiseTurnWeight == 4 ? clockwiseTurnWeight : -clockwiseTurnWeight;
            }

            private static ArgumentException GetPositionIsNotOnLoopException(string chainEndName, Vector2Int position, GridDirection loopLineDirection)
            {
                return new ArgumentException($"Chain {chainEndName} {position} is not on loop (with loop line direction {loopLineDirection})");
            }
        }
#endif
    }
}