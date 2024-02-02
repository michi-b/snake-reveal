namespace Game.Enums.Extensions
{
    public static class GridAxisExtensions
    {
        public static GridAxis GetOther(this GridAxis target)
        {
            return target switch
            {
                GridAxis.Horizontal => GridAxis.Vertical,
                GridAxis.Vertical => GridAxis.Horizontal,
                _ => throw new System.ArgumentOutOfRangeException(nameof(target), target, null)
            };
        }
    }
}