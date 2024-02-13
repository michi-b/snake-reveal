namespace Game.Enums.Extensions
{
    public static class GridDirectionsExtension
    {
        public static bool Contains(this GridDirections directions, GridDirection direction)
        {
            return (directions & direction.AsFlag()) != 0;
        }
    }
}