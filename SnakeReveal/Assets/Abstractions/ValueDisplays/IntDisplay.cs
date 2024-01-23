namespace Abstractions.ValueDisplays
{
    public class IntDisplay : ValueDisplay<int>
    {
        protected override void Apply()
        {
            Renderer.Text = Value.ToString();
        }
    }
}
