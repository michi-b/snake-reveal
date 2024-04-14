using UnityEngine;

namespace TextDisplay
{
    public class TextFormatFloatDisplay : FormatFloatDisplay
    {
        [SerializeField, TextArea] private string _textFormat = "{0} Units";

        protected override void Apply()
        {
            Renderer.Text = string.Format(_textFormat, FormatValue(Value));
        }
    }
}