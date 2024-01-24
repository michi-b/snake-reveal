using System;
using TextDisplay.Abstractions;
using UnityEngine;

namespace TextDisplay
{
    public class FloatAsFullPercentageDisplay : FloatDisplay
    {
        [SerializeField] private RoundingMethod _roundingMethod;
        
        protected override void Apply()
        {
            int percent = _roundingMethod switch
            {
                RoundingMethod.Floor => Mathf.FloorToInt(Value * 100),
                RoundingMethod.Round => Mathf.RoundToInt(Value * 100),
                _ => throw new ArgumentOutOfRangeException()
            };
#if DEBUG
            Debug.Assert(percent >= 0);
            Debug.Assert(percent <= 100);
#endif
            Renderer.Text = PercentageStrings[percent];
        }

        static FloatAsFullPercentageDisplay()
        {
            PercentageStrings = new string[101];
            for (int i = 0; i <= 100; i++)
            {
                PercentageStrings[i] = $"{i} %";
            }
        }
        
        private static readonly string[] PercentageStrings;
        
        private enum RoundingMethod
        {
            Floor,
            Round,
        }
    }
}