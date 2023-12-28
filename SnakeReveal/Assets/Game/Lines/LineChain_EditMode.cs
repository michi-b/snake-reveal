using Game.Enums;

namespace Game.Lines
{
    public partial class LineChain
    {
        public void EditModeReevaluateClockwiseTurnWeight()
        {
            var lastDirection = GridDirection.None;

            if (_loop)
            {
                for (int i = Count - 1; i > 0; i--)
                {
                    lastDirection = _lines[i].Direction;
                    if (lastDirection != GridDirection.None)
                    {
                        break;
                    }
                }
            }

            // sum of clockwise turns - sum of counter clockwise turns
            int clockwiseWeight = 0;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            // not using linq to avoid allocation
            foreach (Line line in _lines)
            {
                if (line.Direction == GridDirection.None)
                {
                    continue;
                }

                if (lastDirection != GridDirection.None)
                {
                    Turn turn = lastDirection.GetTurn(line.Direction);
                    clockwiseWeight += turn.GetClockwiseWeight();
                }

                lastDirection = line.Direction;
            }

            _clockwiseTurnWeight = clockwiseWeight;
        }

        public void EditModeRebuildLineRenderers()
        {
            _renderPointsBuffer.Clear();

            if (Count > 0)
            {
                foreach (Line corner in _lines)
                {
                    _renderPointsBuffer.Add(Grid.GetScenePosition(corner.Start));
                }

                //special handling of open chain end position
                if (!_loop)
                {
                    _renderPointsBuffer.Add(Grid.GetScenePosition(_lines[^1].End));
                }
            }

            foreach (LineChainRenderer lineChainRenderer in _lineRenderers)
            {
                lineChainRenderer.EditModeRebuild(_renderPointsBuffer, Loop);
            }
        }

        public void EditModeFixLines()
        {
            if (Count == 0)
            {
                return;
            }

            for (int i = 0; i < _lines.Count; i++)
            {
                _lines[i] = _lines[i].Clamp(Grid);
            }

            for (int i = 0; i < Count - 1; i++)
            {
                _lines[i] = _lines[i].WithEnd(_lines[i + 1].Start).AsOpenChainEnd(false);
            }

            // special handling of last line based on loop
            if (Loop)
            {
                _lines[^1] = _lines[^1].WithEnd(_lines[0].Start).AsOpenChainEnd(false);
            }
            else
            {
                _lines[^1] = _lines[^1].AsOpenChainEnd(true);
            }
        }
    }
}