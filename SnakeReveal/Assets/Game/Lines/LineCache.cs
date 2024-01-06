namespace Game.Lines
{
    public class LineCache : SimpleCache<Line>
    {
        protected override Line GetCached()
        {
            Line line = base.GetCached();
            line.Initialize();
            return line;
        }
    }
}