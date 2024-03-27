using UnityEngine;

namespace TextDisplay.Abstractions
{
    public abstract class TextRenderer : MonoBehaviour
    {
        public abstract string Text { get; set; }
    }
}