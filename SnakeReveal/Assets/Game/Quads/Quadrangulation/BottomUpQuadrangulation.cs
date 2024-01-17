using System;
using System.Collections.Generic;
using Game.Enums;
using Game.Lines;
using Game.Lines.Insertion;
using UnityEngine;

namespace Game.Quads.Quadrangulation
{
    public class BottomUpQuadrangulation
    {
        private const int DefaultCapacity = 1000;

        private static Comparison<BottomUpQuadrangulationLine> _comparison = (x, y) => y.Y - x.Y;
        private readonly List<Curtain> _curtains;

        private readonly List<BottomUpQuadrangulationLine> _lines;
        private readonly List<QuadData> _result;

        public BottomUpQuadrangulation(int capacity = DefaultCapacity)
        {
            _lines = new List<BottomUpQuadrangulationLine>(capacity);
            _curtains = new List<Curtain>(capacity);
            _result = new List<QuadData>(capacity);
        }

        public IReadOnlyList<QuadData> Evaluate(InsertionEvaluation.InsertionLoopView loop)
        {
            _lines.Clear();

            GridDirection openingDirection = loop.Turn switch
            {
                Turn.None => throw new ArgumentOutOfRangeException(),
                Turn.Right => GridDirection.Left,
                Turn.Left => GridDirection.Right,
                _ => throw new ArgumentOutOfRangeException()
            };

            GridDirection closingDirection = openingDirection.Reverse();

            foreach (LineData line in loop)
            {
                if (BottomUpQuadrangulationLine.TryConvert(line, openingDirection, closingDirection, out BottomUpQuadrangulationLine result))
                {
                    _lines.Add(result);
                }
            }

            _lines.Sort();

            Quadrangulate();

            return _result;
        }

        private void Quadrangulate()
        {
            _curtains.Clear();
            _result.Clear();

            Debug.Assert(_lines.Count >= 2);
            if (!_lines[0].IsOpening)
            {
                throw new InvalidOperationException("First line must be opening");
            }

            _curtains.Add(_lines[0].Curtain);

            for (int i = 1; i < _lines.Count; i++)
            {
                ContinueQuadrangulation(_lines[i]);
            }
        }

        private void ContinueQuadrangulation(BottomUpQuadrangulationLine line)
        {
            Debug.Assert(_curtains.Count > 0);

            if (line.IsOpening)
            {
                Curtain curtain = line.Curtain;
                int insertionIndex = GetCurtainInsertionIndex(line, out ExtensionKind extensionKind);
                InsertCurtain(insertionIndex, extensionKind, curtain);
            }
        }

        private int GetCurtainInsertionIndex(BottomUpQuadrangulationLine line, out ExtensionKind extensionKind)
        {
            Debug.Assert(_curtains.Count > 0);
            
            if(line.Right < _curtains[0].Left)
            {
                extensionKind = ExtensionKind.Insert;
                return 0;
            }

            int currentIndex = 1;
            while (currentIndex < _curtains.Count)
            {
                Curtain curtain = _curtains[currentIndex];
                if (line.Right < curtain.Left)
                {
                    extensionKind = ExtensionKind.Insert;
                    return currentIndex;
                }
                if (line.Right == curtain.Left)
                {
                    extensionKind = ExtensionKind.Left;
                    return currentIndex;
                }
                if (line.Left == curtain.Right)
                {
                    if(currentIndex == _curtains.Count - 1)
                    {
                        // on last index, there cannot be a merge, and it also cannot be checked
                        extensionKind = ExtensionKind.Right;
                    }
                    else
                    {
                        Curtain nextCurtain = _curtains[currentIndex + 1];
                        extensionKind = line.Right == nextCurtain.Left ? ExtensionKind.Merge : ExtensionKind.Right;
                    }
                    return currentIndex;
                }
                currentIndex++;
            }
            
            extensionKind = ExtensionKind.Insert;
            return _curtains.Count;
        }

        private void InsertCurtain(int index, ExtensionKind extensionKind, Curtain newCurtain)
        {
            switch (extensionKind)
            {
                case ExtensionKind.Left:
                    CloseCurtain(_curtains[index], newCurtain.Y);
                    _curtains[index] = new Curtain(newCurtain.Left, _curtains[index].Right, newCurtain.Y);
                    break;
                case ExtensionKind.Right:
                    CloseCurtain(_curtains[index], newCurtain.Y);
                    _curtains[index] = new Curtain(_curtains[index].Left, newCurtain.Right, newCurtain.Y);
                    break;
                case ExtensionKind.Merge:
                    Curtain nextCurtain = _curtains[index + 1];
                    CloseCurtain(_curtains[index], newCurtain.Y);
                    CloseCurtain(nextCurtain, newCurtain.Y);
                    _curtains[index] = new Curtain(_curtains[index].Left, nextCurtain.Right, newCurtain.Y);
                    _curtains.RemoveAt(index + 1);
                    break;
                case ExtensionKind.Insert:
                    _curtains.Insert(index, newCurtain);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(extensionKind), extensionKind, null);
            }
        }

        private void CloseCurtain(Curtain bottomUpQuadrangulationLine, int height)
        {
            _result.Add(new QuadData(bottomUpQuadrangulationLine.Left, bottomUpQuadrangulationLine.Right, bottomUpQuadrangulationLine.Y, height));
        }

        private enum ExtensionKind
        {
            Left,
            Right,
            Insert, // insert curtain at this index
            Merge // merge this index to the next index
        }
    }
}