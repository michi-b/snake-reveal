using TextDisplay.Abstractions;
using UnityEngine;
using UnityEngine.UI;

namespace TextDisplay
{
    [RequireComponent(typeof(Text))]
    public class UITextRenderer : TextRenderer
    {
        [SerializeField] private Text _renderer;

        public override string Text
        {
            get => _renderer.text;
            set => _renderer.text = value;
        }

        protected void Reset()
        {
            _renderer = GetComponent<Text>();
        }
    }
}