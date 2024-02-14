namespace Game.Enums.Utility
{
    public static class GridDirectionUtility
    {
        static GridDirectionUtility()
        {
            ActualDirections = new[]
            {
                GridDirection.Up,
                GridDirection.Right,
                GridDirection.Down,
                GridDirection.Left
            };
        }

        // all defined directions without <see cref="GridDirection.None"/>
        public static readonly GridDirection[] ActualDirections;
    }
}