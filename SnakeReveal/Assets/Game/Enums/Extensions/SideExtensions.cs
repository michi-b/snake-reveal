using System;
using UnityEngine;

namespace Game.Enums.Extensions
{
    public static class SideExtensions
    {
        public static int GetClockwiseTurnWeight(this GridSide side, GridSide other)
        {
#if DEBUG
            Debug.Assert(side != GridSide.None && other != GridSide.None);
#endif
            return (other - side + 4) % 4;
        }

        public static GridDirection GetDirection(this GridSide side, Turn turn)
        {
            return turn switch
            {
                Turn.None => throw new ArgumentOutOfRangeException(nameof(turn), turn, null),
                Turn.Right => side.GetClockwiseDirection(),
                Turn.Left => side.GetClockwiseDirection().Reverse(),
                _ => throw new ArgumentOutOfRangeException(nameof(turn), turn, null)
            };
        }

        public static GridDirection GetOutsideDirection(this GridSide side)
        {
            return side switch
            {
                GridSide.None => throw new ArgumentOutOfRangeException(nameof(side), side, null),
                GridSide.Bottom => GridDirection.Down,
                GridSide.Left => GridDirection.Left,
                GridSide.Top => GridDirection.Up,
                GridSide.Right => GridDirection.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        public static GridAxis GetLineAxis(this GridSide side)
        {
            return side switch
            {
                GridSide.None => throw new ArgumentOutOfRangeException(nameof(side), side, null),
                GridSide.Bottom => GridAxis.Horizontal,
                GridSide.Left => GridAxis.Vertical,
                GridSide.Top => GridAxis.Horizontal,
                GridSide.Right => GridAxis.Vertical,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        private static GridDirection GetClockwiseDirection(this GridSide gridSide)
        {
            return gridSide switch
            {
                GridSide.None => throw new ArgumentOutOfRangeException(nameof(gridSide), gridSide, null),
                GridSide.Bottom => GridDirection.Left,
                GridSide.Left => GridDirection.Up,
                GridSide.Top => GridDirection.Right,
                GridSide.Right => GridDirection.Down,
                _ => throw new ArgumentOutOfRangeException(nameof(gridSide), gridSide, null)
            };
        }
    }
}