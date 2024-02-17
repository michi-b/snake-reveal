namespace Utility
{
    public static class MathUtility
    {
        public const float UnitDiagonal = 0.70710678118f; // 1 / sqrt(2)

        /// <summary>
        ///     same as the % operator, but always returns a positive value (as long as max is > 0)
        /// </summary>
        public static int Loop(int value, int max) => (value + max) % max;
    }
}