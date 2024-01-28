using System.Globalization;
using TextDisplay.Abstractions;
using UnityEngine;

namespace TextDisplay
{
    public class GamePercentCompletionDisplay : FloatDisplay
    {
        // ReSharper disable StringLiteralTypo
        [SerializeField, TextArea] private string _textFormat = "<mspace=0.5em>{0}<alpha=#88></mspace>.<mspace=0.5em>{1}<mspace=1em><alpha=#FF>%";

        // ReSharper restore StringLiteralTypo

        protected override void Apply()
        {
            float percent = Value * 100;
            int full = (int)percent;
            int fraction = (int)((percent - full) * 100);
            Renderer.Text = string.Format(_textFormat, full.ToString(CultureInfo.InvariantCulture), fraction.ToString("D2", CultureInfo.InvariantCulture));
        }
    }
}