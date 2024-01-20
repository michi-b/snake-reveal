using System.Diagnostics;

namespace Game.Quads.Quadrangulation
{
    /// <summary>
    ///     Utility struct for quadrangulation that represents a strictly horizontal shape opening or closing line
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly struct Curtain
    {
        public readonly int Y;
        public readonly int Left;
        public readonly int Right;

        public Curtain(int left, int right, int y)
        {
            Left = left;
            Right = right;
            Y = y;
        }
        
        public override string ToString()
        {
            return $"{Left} -> {Right} @ {Y}";
        }
    }
}