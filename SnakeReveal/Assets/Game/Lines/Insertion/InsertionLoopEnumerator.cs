using System;
using System.Collections.Generic;

namespace Game.Lines.Insertion
{
    public struct InsertionLoopEnumerator
    {
        private enum State
        {
            Initial,
            Chain,
            LoopConnection
        }
        
        private IReadOnlyList<LineData> _chain;
        private IReadOnlyList<LineData> _loopConnection;
        private State _state;
        private int _index;

        public InsertionLoopEnumerator(IReadOnlyList<LineData> chain, IReadOnlyList<LineData> loopConnection)
        {
            _chain = chain;
            _loopConnection = loopConnection;
            _state = State.Initial;
            _index = -1;
        }

        public bool MoveNext()
        {
            return _state switch
            {
                State.Initial => TryInitializeChain() || TryInitializeLoop(),
                State.Chain => ++_index < _chain.Count || TryInitializeLoop(),
                State.LoopConnection => ++_index < _loopConnection.Count,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private bool TryInitializeChain()
        {
            if (_chain.Count <= 0)
            {
                return false;
            }

            _state = State.Chain;
            _index = 0;
            return true;
        }
        
        private bool TryInitializeLoop()
        {
            if (_loopConnection.Count <= 0)
            {
                return false;
            }

            _state = State.LoopConnection;
            _index = 0;
            return true;
        }
        
        public void Reset()
        {
            _state = State.Initial;
            _index = -1;
        }

        public LineData Current => _state switch
        {
            State.Initial => throw new InvalidOperationException(),
            State.Chain => _chain[_index],
            State.LoopConnection => _loopConnection[_index],
            _ => throw new ArgumentOutOfRangeException()
        };

        public void Dispose()
        {
            _chain = null;
            _loopConnection = null;
        }
    }
}