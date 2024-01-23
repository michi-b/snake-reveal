using TextDisplay.Abstractions;

namespace TextDisplay
{
    public class ForwardIntDisplay : IntDisplay
    {
        protected override void Apply()
        {
            Renderer.Text = Value.ToString();
        }
    }
}