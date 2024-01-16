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
            Debug.Assert(_lines[0].IsOpening);

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
                if (TryGetCurtainExtensionIndex(line, out int index, out ExtensionKind extensionKind))
                {
                    ExtendCurtain(index, extensionKind, curtain);
                }
            }
        }

        private void ExtendCurtain(int index, ExtensionKind extensionKind, Curtain extension)
        {
            Curtain curtain = _curtains[index];

            CloseCurtain(curtain, extension.Y);

            switch (extensionKind)
            {
                case ExtensionKind.Left:
                    _curtains[index] = new Curtain(extension.Left, curtain.Right, extension.Y);
                    break;
                case ExtensionKind.Right:
                    _curtains[index] = new Curtain(curtain.Left, extension.Right, extension.Y);
                    break;
                case ExtensionKind.Merge:
                    Curtain nextCurtain = _curtains[index + 1];
                    CloseCurtain(nextCurtain, extension.Y);
                    _curtains[index] = new Curtain(curtain.Left, nextCurtain.Right, extension.Y);
                    _curtains.RemoveAt(index + 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(extensionKind), extensionKind, null);
            }
        }

        private void CloseCurtain(Curtain bottomUpQuadrangulationLine, int height)
        {
            _result.Add(new QuadData(bottomUpQuadrangulationLine.Left, bottomUpQuadrangulationLine.Right, bottomUpQuadrangulationLine.Y, height));
        }

        private bool TryGetCurtainExtensionIndex(BottomUpQuadrangulationLine line, out int index, out ExtensionKind kind)
        {
            int lastIndex = _curtains.Count - 1;

            for (int i = 1; i < lastIndex; i++)
            {
                Curtain curtain = _curtains[i];

                if (line.Right == curtain.Left)
                {
                    kind = ExtensionKind.Left;
                    index = i;
                    return true;
                }

                if (line.Left == curtain.Right)
                {
                    Curtain nextCurtain = _curtains[i + 1];

                    if (line.Right == nextCurtain.Left)
                    {
                        kind = ExtensionKind.Merge;
                        index = i;
                        return true;
                    }

                    kind = ExtensionKind.Right;
                    index = 0;
                    return true;
                }
            }

            // last index cannot merge
            Curtain lastCurtain = _curtains[^1];
            if (line.Right == lastCurtain.Left)
            {
                kind = ExtensionKind.Left;
                index = lastIndex;
                return true;
            }

            if (line.Left == lastCurtain.Right)
            {
                kind = ExtensionKind.Right;
                index = lastIndex;
                return true;
            }

            index = -1;
            kind = default;
            return false;
        }

        private enum ExtensionKind
        {
            Left,
            Right,
            Merge // merge this index to the next index
        }
    }
}