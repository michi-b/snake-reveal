using TextDisplay.Abstractions;
using TMPro;
using UnityEngine;

namespace TextDisplay
{
    [RequireComponent(typeof(TMP_Text))]
    public class TmpTextRenderer : TextRenderer
    {
        [SerializeField] private TMP_Text _renderer;

        public override string Text
        {
            get => _renderer.text;
            set => _renderer.text = value; 
        }

        protected void Reset()
        {
            _renderer = GetComponent<TMP_Text>();
        }
    }
}