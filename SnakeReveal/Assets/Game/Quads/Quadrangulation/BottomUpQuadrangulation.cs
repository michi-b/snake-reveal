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

        private readonly List<Curtain> _curtains;

        private readonly List<CurtainEndLine> _lines;
        private readonly List<QuadData> _result;

        public BottomUpQuadrangulation(int capacity = DefaultCapacity)
        {
            _lines = new List<CurtainEndLine>(capacity);
            _curtains = new List<Curtain>(capacity);
            _result = new List<QuadData>(capacity);
        }

        public List<QuadData> Evaluate(LineLoop loop)
        {
            SetLines(loop);
            Quadrangulate();
            return _result;
        }

        public List<QuadData> Evaluate(InsertionEvaluation.InsertionLoopView loop)
        {
            SetLines(loop);
            Quadrangulate();
            return _result;
        }

        private static GridDirection GetOpeningDirection(Turn turn)
        {
            return turn switch
            {
                Turn.None => throw new ArgumentOutOfRangeException(),
                Turn.Right => GridDirection.Left,
                Turn.Left => GridDirection.Right,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void SetLines(LineLoop loop)
        {
            GridDirection openingDirection = GetOpeningDirection(loop.Turn);
            GridDirection closingDirection = openingDirection.Reverse();
            
            _lines.Clear();
            foreach (Line line in loop.AsSpan())
            {
                if (CurtainEndLine.TryConvert(line.Data, openingDirection, closingDirection, out CurtainEndLine result))
                {
                    _lines.Add(result);
                }
            }

            _lines.Sort();
        }

        private void SetLines(InsertionEvaluation.InsertionLoopView loop)
        {
            GridDirection openingDirection = GetOpeningDirection(loop.Turn);
            GridDirection closingDirection = openingDirection.Reverse();
            
            _lines.Clear();
            foreach (LineData line in loop)
            {
                if (CurtainEndLine.TryConvert(line, openingDirection, closingDirection, out CurtainEndLine result))
                {
                    _lines.Add(result);
                }
            }

            _lines.Sort();
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

        private void ContinueQuadrangulation(CurtainEndLine line)
        {
            Debug.Assert(_curtains.Count > 0);

            if (line.IsOpening)
            {
                Curtain opener = line.Curtain;
                int insertionIndex = GetCurtainInsertionIndex(opener, out InsertionKind insertionKind);
                InsertCurtain(insertionIndex, insertionKind, opener);
            }
            else // line.IsClosing
            {
                Curtain closure = line.Curtain;
                int toCloseIndex = GetCurtainToCloseIndex(closure);
                Curtain toClose = _curtains[toCloseIndex];
                
                AddCurtainCloseQuad(toClose, closure.Y);

                if (closure.Left == toClose.Left)
                {
                    if (closure.Right == toClose.Right)
                    {
                        // exact match
                        _curtains.RemoveAt(toCloseIndex);
                        return;
                    }

                    // left side close
                    _curtains[toCloseIndex] = new Curtain(closure.Right, toClose.Right, closure.Y);
                }
                else if (closure.Right == toClose.Right)
                {
                    // right side close
                    _curtains[toCloseIndex] = new Curtain(toClose.Left, closure.Left, closure.Y);
                }
                else
                {
                    // split
                    _curtains[toCloseIndex] = new Curtain(toClose.Left, closure.Left, closure.Y);
                    _curtains.Insert(toCloseIndex + 1, new Curtain(closure.Right, toClose.Right, closure.Y));
                }
            }
        }

        private int GetCurtainInsertionIndex(Curtain opener, out InsertionKind insertionKind)
        {
            Debug.Assert(_curtains.Count > 0);

            for (int i = 0; i < _curtains.Count; i++)
            {
                Curtain currentCurtain = _curtains[i];
                if (opener.Right < currentCurtain.Left)
                {
                    insertionKind = InsertionKind.Insert;
                    return i;
                }
                if (opener.Right == currentCurtain.Left)
                {
                    insertionKind = InsertionKind.Left;
                    return i;
                }
                if (opener.Left == currentCurtain.Right)
                {
                    if(i == _curtains.Count - 1)
                    {
                        // on last index, there cannot be a merge, and it also cannot be checked
                        insertionKind = InsertionKind.Right;
                    }
                    else
                    {
                        Curtain nextCurtain = _curtains[i + 1];
                        insertionKind = opener.Right == nextCurtain.Left ? InsertionKind.Merge : InsertionKind.Right;
                    }
                    return i;
                }
            }
            
            insertionKind = InsertionKind.Insert;
            return _curtains.Count;
        }

        private void InsertCurtain(int insertionIndex, InsertionKind insertionKind, Curtain newCurtain)
        {
            switch (insertionKind)
            {
                case InsertionKind.Left:
                    AddCurtainCloseQuad(_curtains[insertionIndex], newCurtain.Y);
                    _curtains[insertionIndex] = new Curtain(newCurtain.Left, _curtains[insertionIndex].Right, newCurtain.Y);
                    break;
                case InsertionKind.Right:
                    AddCurtainCloseQuad(_curtains[insertionIndex], newCurtain.Y);
                    _curtains[insertionIndex] = new Curtain(_curtains[insertionIndex].Left, newCurtain.Right, newCurtain.Y);
                    break;
                case InsertionKind.Merge:
                    Curtain nextCurtain = _curtains[insertionIndex + 1];
                    AddCurtainCloseQuad(_curtains[insertionIndex], newCurtain.Y);
                    AddCurtainCloseQuad(nextCurtain, newCurtain.Y);
                    _curtains[insertionIndex] = new Curtain(_curtains[insertionIndex].Left, nextCurtain.Right, newCurtain.Y);
                    _curtains.RemoveAt(insertionIndex + 1);
                    break;
                case InsertionKind.Insert:
                    _curtains.Insert(insertionIndex, newCurtain);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(insertionKind), insertionKind, null);
            }
        }

        private int GetCurtainToCloseIndex(Curtain closure)
        {
            for (int i = 0; i < _curtains.Count; i++)
            {
                Curtain currentCurtain = _curtains[i];
                Debug.Assert(closure.Left >= currentCurtain.Left, "If the close is left of the current curtain, it should have closed an earlier curtain already");
                if(closure.Left >= currentCurtain.Left && closure.Right <= currentCurtain.Right)
                {
                    // close somehow closes the current curtain
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException(nameof(closure), closure, "Close does not close any curtain");
        }

        private void AddCurtainCloseQuad(Curtain toClose, int closureHeight)
        {
            _result.Add(new QuadData(toClose.Left, toClose.Right, toClose.Y, closureHeight));
        }

        private enum InsertionKind
        {
            Left,
            Right,
            Insert, // insert curtain at this index
            Merge // merge this index to the next index
        }
    }
}