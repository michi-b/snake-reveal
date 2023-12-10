using System;

namespace Game.Enums
{
    public static class TurnExtensions
    {
        public static int GetClockwiseWeight(this Turn turn)
        {
            return turn switch
            {
                Turn.Clockwise => 1,
                Turn.CounterClockwise => -1,
                _ => throw new ArgumentOutOfRangeException(nameof(turn), turn, null)
            };
        }

        public static Turn GetOpposite(this Turn turn)
        {
            return turn switch
            {
                Turn.None => Turn.None,
                Turn.Clockwise => Turn.CounterClockwise,
                Turn.CounterClockwise => Turn.Clockwise,
                _ => throw new ArgumentOutOfRangeException(nameof(turn), turn, null)
            };
        }
    }
}