using UnityEngine;

namespace Game.Menu.ValueDisplays
{
    public abstract class TextRenderer : MonoBehaviour
    {
        [SerializeField] private string _text;

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
