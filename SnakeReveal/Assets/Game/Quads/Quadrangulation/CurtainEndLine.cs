using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Game.Enums;
using Game.Lines;

namespace Game.Quads.Quadrangulation
{
    /// <summary>
    ///     Utility struct for quadrangulation that extends a curtain with the information, whether it is opening or closing
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly struct CurtainEndLine : IComparable<CurtainEndLine>
    {
        private readonly Curtain _curtain;

        public int Left => _curtain.Left;

        public int Right => _curtain.Right;

        public int Y => _curtain.Y;

        public Curtain Curtain => _curtain;

        private CurtainEndLine(LineData line, bool isClosing)
        {
            _curtain = line.Direction switch
            {
                GridDirection.None => throw new ArgumentOutOfRangeException(),
                GridDirection.Right => new Curtain(line.Start.x, line.End.x, line.Start.y),
                GridDirection.Up => throw new ArgumentOutOfRangeException(),
                GridDirection.Left => new Curtain(line.End.x, line.Start.x, line.Start.y),
                GridDirection.Down => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            };
            IsOpening = !isClosing;
        }

        public bool IsOpening { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryConvert(LineData line, GridDirection openingDirection, GridDirection closingDirection, out CurtainEndLine result)
        {
            if (line.Direction == openingDirection)
            {
                result = new CurtainEndLine(line, false);
                return true;
            }

            if (line.Direction == closingDirection)
            {
                result = new CurtainEndLine(line, true);
                return true;
            }

            result = default;
            return false;
        }

        public int CompareTo(CurtainEndLine other)
        {
            return Y - other.Y;
        }

        public override string ToString()
        {
            return $"{(IsOpening ? "Opening" : "Closing")} {_curtain}";
        }
    }
}