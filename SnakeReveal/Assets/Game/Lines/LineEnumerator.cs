using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Game.Lines
{
    public struct LineEnumerator : IEnumerator<Line>
    {
        private readonly Line _first;
        private readonly Options _options;
        public Line Current { get; private set; }

        public LineEnumerator(Line first, Options options)
        {
            _first = first;
            _options = options;
            Current = null;
        }

        public bool MoveNext()
        {
            // empty case
            if (_first == null)
            {
                return false;
            }

            if (Current == null)
            {
                Current = _first;
                bool skipFirst = ((int)_options & (int)Options.SkipFirst) != 0;
                if (!skipFirst)
                {
                    return true;
                }
            }

            Current = Current.Next;
            
            if(Current == null
               || Current == _first)
            {
                return false;
            }
            
            bool skipLast = ((int)_options & (int)Options.SkipLast) != 0;
            
            return !skipLast || (Current.Next != null && Current.Next != _first);
        }

        public void Reset()
        {
            Current = _first;
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        [Flags]
        public enum Options
        {
            None = 0,
            SkipFirst = 1 << 0,
            SkipLast = 1 << 1,
            SkipFirstAndLast = SkipFirst | SkipLast
        }
    }
}