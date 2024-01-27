using UnityEngine;

namespace TextDisplay.Abstractions
{
    public abstract class TextRenderer : MonoBehaviour
    {
        [SerializeField, TextArea] private string _text;

        public string Text
        {
            protected get => _text;
            set
            {
                _text = value;
                Apply();
            }
        }

        protected abstract void Apply();
    }
}