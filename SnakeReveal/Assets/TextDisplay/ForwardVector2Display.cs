using TextDisplay.Abstractions;

namespace TextDisplay
{
    public class ForwardVector2Display : Vector2Display
    {
        protected override void Apply()
        {
            Renderer.Text = Value.ToString();
        }
    }
}