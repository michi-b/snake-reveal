using UnityEngine;

namespace Abstractions.ValueDisplays
{
    public class FloatDisplay : ValueDisplay<float>
    {
        [SerializeField] private string _format = "F2";
        
        protected override void Apply()
        {
            Renderer.Text = Value.ToString(_format);
        }
    }
}
