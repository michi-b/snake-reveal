using System;
using System.Runtime.CompilerServices;
using Game.Enums;
using Game.Lines;

namespace Game.Quads.Quadrangulation
{
    public struct BottomUpQuadrangulationLine : IComparable<BottomUpQuadrangulationLine>
    {
        public int Y;
        public int Left;
        public int Right;
        public QuadrangulationLineKind Kind;

        private BottomUpQuadrangulationLine(LineData line, QuadrangulationLineKind kind)
        {
            switch (line.Direction)
            {
                case GridDirection.None:
                    throw new ArgumentOutOfRangeException();
                case GridDirection.Right:
                    Y = line.Start.y;
                    Left = line.Start.x;
                    Right = line.End.x;
                    break;
                case GridDirection.Up:
                    throw new ArgumentOutOfRangeException();
                case GridDirection.Left:
                    Y = line.Start.y;
                    Left = line.End.x;
                    Right = line.Start.x;
                    break;
                case GridDirection.Down:
                    throw new ArgumentOutOfRangeException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Kind = kind;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryConvert(LineData line, GridDirection openingDirection, GridDirection closingDirection, out BottomUpQuadrangulationLine result)
        {
            if (line.Direction == openingDirection)
            {
                result = new BottomUpQuadrangulationLine(line, QuadrangulationLineKind.Opening);
                return true;
            }

            if (line.Direction == closingDirection)
            {
                result = new BottomUpQuadrangulationLine(line, QuadrangulationLineKind.Closing);
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