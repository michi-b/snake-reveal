using System.Collections;
using System.Collections.Generic;

namespace Game.Lines
{
    public readonly struct LineEnumerable : IEnumerable<Line>
    {
        private readonly Line _line;
        private readonly LineEnumerator.Options _options;

        public LineEnumerable(Line line, LineEnumerator.Options options = LineEnumerator.Options.None)
        {
            _line = line;
            _options = options;
        }

        public LineEnumerator GetEnumerator()
        {
            return new LineEnumerator(_line, _options);
        }

        IEnumerator<Line> IEnumerable<Line>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}