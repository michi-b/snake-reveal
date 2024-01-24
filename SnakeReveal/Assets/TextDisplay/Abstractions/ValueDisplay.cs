using UnityEngine;

namespace TextDisplay.Abstractions
{
    public abstract class ValueDisplay<TValue> : MonoBehaviour
    {
        [SerializeField] private TValue _value;
        [SerializeField] private TextRenderer _renderer;

        protected TextRenderer Renderer => _renderer;

        public TValue Value
        {
            protected get => _value;
            set
            {
                _value = value;
                Apply();
            }
        }
        
        protected abstract void Apply();
    }
}