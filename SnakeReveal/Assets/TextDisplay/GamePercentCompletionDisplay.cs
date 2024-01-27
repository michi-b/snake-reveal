using System;
using System.Globalization;
using TextDisplay.Abstractions;
using UnityEngine;

namespace TextDisplay
{
    public class GamePercentCompletionDisplay : FloatDisplay
    {
        // ReSharper disable StringLiteralTypo
        [SerializeField] private string _textFormat = "<mspace=0.5em>{0}<mspace=1em>%";
        // ReSharper restore StringLiteralTypo

        private const int DecimalPlaces = 2;
        private static readonly double TruncationFactor = Math.Pow(10, DecimalPlaces);
        private static readonly string FloatFormat = $"F{DecimalPlaces}";


        protected override void Apply()
        {
            float percent = Value * 100;
            float truncatedPercent = (float)(Math.Truncate(percent * TruncationFactor) / TruncationFactor);
            Renderer.Text = string.Format(_textFormat, truncatedPercent.ToString(FloatFormat, CultureInfo.InvariantCulture));
        }
    }
}