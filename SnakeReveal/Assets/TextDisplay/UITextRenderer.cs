using TextDisplay.Abstractions;
using UnityEngine;
using UnityEngine.UI;

namespace TextDisplay
{
    [RequireComponent(typeof(Text))]
    public class UITextRenderer : TextRenderer
    {
        [SerializeField] private Text _renderer;

        protected void Reset()
        {
            _renderer = GetComponent<Text>();
        }

        protected override void Apply()
        {
            _renderer.text = Text;
        }
    }
}