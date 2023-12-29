using System;
using UnityEngine;

namespace Game.Enums
{
    public static class TurnExtensions
    {
        public static int GetClockwiseWeight(this Turn turn)
        {
            return turn switch
            {
                Turn.Right => 1,
                Turn.Left => -1,
                _ => 0
            };
        }

        public static int GetWeight(this Turn target, Turn turn)
        {
#if DEBUG
            if (target == turn)
            {
                return 1;
            }

            Debug.Assert(target == turn.Reverse());
            return -1;
#else
            return turn == target ? 1 : -1;
#endif
        }

        public static Turn Reverse(this Turn turn)
        {
            return turn switch
            {
                Turn.None => Turn.None,
                Turn.Right => Turn.Left,
                Turn.Left => Turn.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(turn), turn, null)
            };
        }
    }
}