using TextDisplay.Abstractions;
using UnityEngine;

namespace TextDisplay
{
    public class FormatFloatDisplay : FloatDisplay
    {
        [SerializeField] private string _format = "F2";

        protected override void Apply()
        {
            Renderer.Text = FormatValue(Value);
        }

        protected virtual string FormatValue(float value)
        {
            return value.ToString(_format);
        }
    }
}