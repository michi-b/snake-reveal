using System;
using System.Runtime.CompilerServices;
using Game.Enums;
using Game.Lines;

namespace Game.Quads.Quadrangulation
{
    /// <summary>
    ///     Utility struct for quadrangulation that extends a curtain with the information, whether it is opening or closing
    /// </summary>
    public readonly struct BottomUpQuadrangulationLine : IComparable<BottomUpQuadrangulationLine>
    {
        private readonly Curtain _curtain;

        public int Left => _curtain.Left;

        public int Right => _curtain.Right;

        public int Y => _curtain.Y;

        public Curtain Curtain => _curtain;

        private BottomUpQuadrangulationLine(LineData line, bool isClosing)
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
            IsClosing = isClosing;
        }

        public bool IsOpening => !IsClosing;

        public bool IsClosing { get; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryConvert(LineData line, GridDirection openingDirection, GridDirection closingDirection, out BottomUpQuadrangulationLine result)
        {
            if (line.Direction == openingDirection)
            {
                result = new BottomUpQuadrangulationLine(line, false);
                return true;
            }

            if (line.Direction == closingDirection)
            {
                result = new BottomUpQuadrangulationLine(line, true);
                return true;
            }

            result = default;
            return false;
        }

        public int CompareTo(BottomUpQuadrangulationLine other)
        {
            return Y - other.Y;
        }
    }
}