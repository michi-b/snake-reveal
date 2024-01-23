using TextDisplay.Abstractions;
using TMPro;
using UnityEngine;

namespace TextDisplay
{
    [RequireComponent(typeof(TMP_Text))]
    public class TmpTextRenderer : TextRenderer
    {
        [SerializeField] private TMP_Text _renderer;

        protected void Reset()
        {
            _renderer = GetComponent<TMP_Text>();
        }

        protected override void Apply()
        {
            _renderer.text = Text;
        }
    }
}