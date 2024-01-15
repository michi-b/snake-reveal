using System;
using System.Collections.Generic;
using Game.Enums;
using Game.Lines;
using Game.Lines.Insertion;

namespace Game.Quads.Quadrangulation
{
    public class BottomUpQuadrangulation
    {
        private const int DefaultCapacity = 1000;
        
        private List<BottomUpQuadrangulationLine> _lines;
        private List<QuadData>  _result;
        
        private static Comparison<BottomUpQuadrangulationLine> _comparison = ((x, y) => y.Y - x.Y);

        public BottomUpQuadrangulation(int capacity = DefaultCapacity)
        {
            _lines = new List<BottomUpQuadrangulationLine>(capacity);
            _result = new List<QuadData>(capacity);
        }

        public IReadOnlyList<QuadData> Evaluate(InsertionEvaluation.InsertionLoopView loop)
        {
            Clear();
            
            GridDirection openingDirection = loop.Turn switch
            {
                Turn.None => throw new ArgumentOutOfRangeException(),
                Turn.Right => GridDirection.Left,
                Turn.Left => GridDirection.Right,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            GridDirection closingDirection = openingDirection.Reverse();

            List<LineData> lineData = new List<LineData>();
            foreach (LineData line in loop)
            {
                lineData.Add(line);
                if(BottomUpQuadrangulationLine.TryConvert(line, openingDirection, closingDirection, out BottomUpQuadrangulationLine result))
                {
                    _lines.Add(result);
                }
            }

            _lines.Sort();
            
            return _result;
        }

        private void Clear()
        {
            _lines.Clear();
            _result.Clear();
        }
    }
}