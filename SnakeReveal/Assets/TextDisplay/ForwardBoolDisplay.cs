using TextDisplay.Abstractions;

namespace TextDisplay
{
    public class ForwardBoolDisplay : BoolDisplay
    {
        protected override void Apply()
        {
            Renderer.Text = Value.ToString();
        }
    }
}